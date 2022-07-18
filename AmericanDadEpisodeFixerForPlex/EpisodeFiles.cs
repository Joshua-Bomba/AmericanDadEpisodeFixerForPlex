using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmericanDadEpisodeFixerForPlex
{
    public class EpisodeFiles : List<EpisodeFile>
    {
        private DirectoryInfo _dir;
        public EpisodeFiles(DirectoryInfo dir)
        {
            _dir = dir;
        }

        public bool ProcessEpisodes()
        {
            string dir = _dir.FullName;
            foreach (EpisodeFile episode in this)
                if (!episode.EstimateEpisode(dir))
                    return false;
            return true;
        }
    }
}
