using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AmericanDadEpisodeFixerForPlex
{
    public class BaseEpisode
    {
        public int? Season { get; set; }
        public int? EpisodeNumber { get; set; }

        public string CombinedEpisodeAndSeason() => string.Format("S{0:00}E{1:00}", Season, EpisodeNumber);
    }

    public class Episode : BaseEpisode
    {
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
}
