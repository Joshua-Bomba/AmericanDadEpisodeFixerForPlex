using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmericanDadEpisodeFixerForPlex
{
    public class EpisodeFileResolver
    {
        public EpisodeFileResolver() 
        {
        
        }

        public string SeriesFolder { get; init; }

        public async ValueTask<bool> PullFilesAndNames()
        {
            if (SeriesFolder != null&&Directory.Exists(SeriesFolder))
            {
                string[] files = Directory.GetFiles(SeriesFolder);
            }
            return false;
        }



    }
}
