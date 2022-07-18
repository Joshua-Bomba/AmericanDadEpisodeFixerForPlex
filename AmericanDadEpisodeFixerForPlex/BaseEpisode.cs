using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmericanDadEpisodeFixerForPlex
{
    public class BaseEpisode
    {
        public int? Season { get; set; }
        public int? EpisodeNumber { get; set; }

        public string CombinedEpisodeAndSeason() => string.Format("S{0:00}E{1:00}", Season, EpisodeNumber);
    }
}
