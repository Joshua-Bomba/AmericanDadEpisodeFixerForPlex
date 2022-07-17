using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace AmericanDadEpisodeFixerForPlex
{
    public class Episode
    {
        public int? Season { get; set; }
        public int? EpisodeNumber { get; set; }

        public string? EpisodeName { get; set; }

        public DateTime? AirDate { get; set; }

        public string? ProductionCode { get; set; }

        private string ProcessElementContent(StringBuilder sb)
        {
            string res = sb.ToString();
            res = Regex.Replace(res, "\r|\n|\"", string.Empty);
            return res;
        }

        public void ProcessContent(int index, StringBuilder sb)
        { 
            string s;
            try
            {
                switch (index)
                {
                    case 0:
                        s = ProcessElementContent(sb);
                        s = Regex.Match(s, "^\\d*").Value;
                        EpisodeNumber = Convert.ToInt32(s);
                        break;
                    case 1:
                        s = ProcessElementContent(sb);
                        EpisodeName = s;
                        break;
                    case 4:
                        s = ProcessElementContent(sb);
                        s = Regex.Match(s, "(?<=(\\()).+(?=\\))").Value;

                        AirDate = Convert.ToDateTime(s);
                        break;
                    case 5:
                        s = ProcessElementContent(sb);
                        ProductionCode = s;
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                sb.Clear();
            }
            
        }


    }


    public class EpisodeFixer : IAsyncDisposable
    {
        public const string TABLE_MATCH = "<table.*wikiepisodetable.*?>(.|\n)*?(<\\/table>)";
        public static readonly HashSet<string> TABLE_TAGS = new HashSet<string> { "table", "th", "tr", "td","caption","colgroup","col","thread","tbody","tfoot" };
        private HttpClient _client;
        private Task _beforeFinish;
        public EpisodeFixer() 
        {
            _client = new HttpClient();
        }

        public string EpisodeDataEndpoint { get; init; }

        public string? CachePage { get; init; }

        private async ValueTask<string?> GetPageData()
        {
            if(CachePage != null)
            {
                if (File.Exists(CachePage))
                {
                    return await File.ReadAllTextAsync(CachePage);
                }
            }
            var response = await _client.GetAsync(EpisodeDataEndpoint);

            if (response.IsSuccessStatusCode)
            {
                 string content = await response.Content.ReadAsStringAsync();
                if(CachePage != null)
                {
                    _beforeFinish = File.WriteAllTextAsync(CachePage, content);
                }
                return content;
            }
            else
            {
                return null;
            }
        }

        private async IAsyncEnumerable<Episode> ProcessMatch(int season, string table)
        {
            using (XmlReader reader = XmlReader.Create(new StringReader(table),new XmlReaderSettings { Async = true}))
            {
                string res;
                Stack<string> element = new Stack<string>();
                StringBuilder elementContent = new StringBuilder();
                Episode row = null;
                //DataTable dt = new DataTable();
                //DataRow row = null;
                int index = 0;
                while (await reader.ReadAsync())
                {

                    switch (reader.NodeType)
                    {

                        case XmlNodeType.Element:
                            res = reader.Name;
                            if(TABLE_TAGS.Contains(res))
                            {
                                element.Push(res);
                            }

                            break;
                        case XmlNodeType.Text:
                            res = await reader.GetValueAsync();
                            elementContent.AppendLine(res);
                            Console.WriteLine("Text Node: {0}",res );
                            break;
                        case XmlNodeType.EndElement:
                            res = reader.Name;
                            if (TABLE_TAGS.Contains(res))
                            {
                                if (res == "th")
                                {
                                    elementContent.Clear();
                                }
                                else
                                if (res == "td")
                                {
                                    if(row == null)
                                    {
                                        row = new Episode { Season = season };
                                    }
                                    row.ProcessContent(index, elementContent);
                                    index++;
                                }
                                else if(res == "tr")
                                {
                                    if(row != null)
                                    {
                                        //dt.Rows.Add(row);
                                        yield return row;
                                        row = null;
                                        index = 0;
                                    }
                                }
                                element.Pop();
                            }
                            Console.WriteLine("End Element {0}", res);
                            break;
                        default:
                            res = reader.Value;
                            Console.WriteLine("Other node {0} with value {1}",
                                            reader.NodeType, res);
                            break;
                    }
                }
            }
        }



        public async ValueTask<bool> ProcessEpisodeData()
        {
            string? content = await GetPageData();
            if(!string.IsNullOrWhiteSpace(content))
            {
                Dictionary<string,Episode> episodes = new Dictionary<string,Episode>();
                MatchCollection tables= Regex.Matches(content, TABLE_MATCH);
                //would like to pull from plex since it's better but i'm not sure how there api works
                //and this is supposto be quick so i'm gonna pull from wikipea and handle it my own way
                int season = 0;
                foreach(Match m in tables)
                {
                    await foreach(Episode e in ProcessMatch(++season, m.Value))
                    {
                        episodes.Add(e.ProductionCode, e);
                    }
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        public async ValueTask DisposeAsync()
        {
            if(_beforeFinish != null)
            {
                await _beforeFinish;
            }
        }
    }
}
