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

        public bool EstimateEpisodes()
        {
            string dir = _dir.FullName;
            foreach (EpisodeFile episode in this)
                if (!episode.EstimateEpisode(dir))
                    return false;
            return true;
        }

        public void CalculateMoves(string? newFolder)
        {
            string name;
            if(newFolder != null)
            {
                if (Path.IsPathRooted(newFolder))
                {
                    name = newFolder;
                }
                else
                {
                    name = Path.Combine(_dir.FullName, newFolder);
                }
            }
            else
            {
                name = _dir.FullName;
            }

            foreach (EpisodeFile episode in this)
                episode.CalculateMove(name);
        }


        public Dictionary<string,EpisodeFile> GetOrderedEpisode()
        {
            return this.GroupBy(x => x.AssociatedEpisode.Season).OrderBy(x => x.Key).SelectMany(x => x.OrderBy(y => y.AssociatedEpisode.EpisodeNumber))
                .ToDictionary(x => x.AssociatedEpisode.CombinedEpisodeAndSeason(), y => y);
        }
    }
}
