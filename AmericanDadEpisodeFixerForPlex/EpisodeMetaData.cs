using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
   
    public class EpisodeMetaData : IAsyncDisposable
    {
        public const string TABLE_MATCH = "<table.*wikiepisodetable.*?>(.|\n)*?(<\\/table>)";
        public static readonly HashSet<string> TABLE_TAGS = new HashSet<string> { "table", "th", "tr", "td","caption","colgroup","col","thread","tbody","tfoot" };
        private HttpClient _client;
        private Stack<Task> _beforeFinish;
        private Dictionary<string, Episode> _episodes;
        public EpisodeMetaData() 
        {
            _episodes =new Dictionary<string, Episode>();
        }

        public string EpisodeDataEndpoint { get; init; }

        public string? CachePage { get; init; }

        public string? CachedEpisodes { get; init; }

        public string? LogOutputFile { get; init; }

        private async ValueTask<string?> GetPageData()
        {
            if(CachePage != null)
            {
                if (File.Exists(CachePage))
                {
                    Console.WriteLine("Using Cached Page");
                    return await File.ReadAllTextAsync(CachePage);
                }
            }
            Console.WriteLine("No Cached Page. Fetching from the web");
            _client = new HttpClient();
            var response = await _client.GetAsync(EpisodeDataEndpoint);

            if (response.IsSuccessStatusCode)
            {
                 string content = await response.Content.ReadAsStringAsync();
                if(CachePage != null)
                {
                    if (_beforeFinish == null)
                        _beforeFinish = new Stack<Task>();
                    _beforeFinish.Push(File.WriteAllTextAsync(CachePage, content));
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
                            break;
                        default:
                            res = reader.Value;
                            break;
                    }
                }
            }
        }


        public async ValueTask<bool> CheckForCachedEpisodes()
        {
            if(File.Exists(CachedEpisodes))
            {
                string content = await File.ReadAllTextAsync(CachedEpisodes);
                _episodes = JsonConvert.DeserializeObject<Dictionary<string, Episode>>(content);
                return true;
            }
            return false;
        }


        public async Task ExportCachedEpisodes()
        {
            await Task.Run(async () =>
            {
                string res = JsonConvert.SerializeObject(_episodes, new JsonSerializerSettings{Formatting = Newtonsoft.Json.Formatting.Indented });

                await File.WriteAllTextAsync(CachedEpisodes, res);


            });
        }

        
        public void OutputLogFile(Dictionary<string, EpisodeFile> assocications)
        {
            if(LogOutputFile != null)
            {
                if (_beforeFinish == null)
                    _beforeFinish = new Stack<Task>();
                _beforeFinish.Push(Task.Run(async () =>
                {
                    string res = JsonConvert.SerializeObject(assocications, new JsonSerializerSettings { Formatting = Newtonsoft.Json.Formatting.Indented });
                    await File.WriteAllTextAsync(LogOutputFile, res);
                }));
            }
        }


        public async ValueTask<bool> ProcessEpisodeData()
        {
            if(!await this.CheckForCachedEpisodes())
            {
                Console.WriteLine("No Cached Episodes Processing from page");
                string? content = await GetPageData();
                if (!string.IsNullOrWhiteSpace(content))
                {
                    MatchCollection tables = Regex.Matches(content, TABLE_MATCH);
                    //would like to pull from plex since it's better but i'm not sure how there api works
                    //and this is supposto be quick so i'm gonna pull from wikipea and handle it my own way
                    int season = 0;
                    foreach (Match m in tables)
                    {
                        await foreach (Episode e in ProcessMatch(++season, m.Value))
                        {
                            _episodes.Add(e.CombinedEpisodeAndSeason(), e);
                        }
                    }

                    if (CachedEpisodes != null)
                    {
                        if (_beforeFinish == null)
                            _beforeFinish = new Stack<Task>();
                        _beforeFinish.Push(ExportCachedEpisodes());
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return true;
            }
        }
        public Dictionary<int,Dictionary<int,Episode>> GetEpisodesForEachSeason()
        {
            return _episodes.Values.GroupBy(x => x.Season.Value).ToDictionary(x=>x.Key,y=>y.ToDictionary(z=>z.EpisodeNumber.Value,zz=>zz));
        }

        public async ValueTask DisposeAsync()
        {
            if(_beforeFinish != null)
            {
                await Task.WhenAll(_beforeFinish);
                _beforeFinish = null;
            }
        }
    }
}
