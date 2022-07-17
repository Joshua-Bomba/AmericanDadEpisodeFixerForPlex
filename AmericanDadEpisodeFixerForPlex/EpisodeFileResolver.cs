using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AmericanDadEpisodeFixerForPlex
{

    public class EpisodeFile : BaseEpisode
    {
        public const string FIND_SEASON = "(?<=(s(eason)? ?))\\d+";
        public const string FIND_EPISODE = "(?<=(e(pisode)? ?))\\d+";

        public FileInfo FileInfo { get; set; }

        public void EstimateEpisode(string baseDir)
        {
            string name = FileInfo.FullName;
            name = name.Replace(baseDir, string.Empty);
            string season = Regex.Match(name, FIND_SEASON,RegexOptions.IgnoreCase).Value;
            string episode = Regex.Match(name, FIND_EPISODE, RegexOptions.IgnoreCase).Value;
            if(!string.IsNullOrWhiteSpace(season)&&!string.IsNullOrWhiteSpace(episode))
            {
                int s = Convert.ToInt32(season);
                int e = Convert.ToInt32(episode);
                EpisodeNumber = e;
                Season = s;
            }
            else
            {

            }
        }

    }


    public class EpisodeFileResolver
    {
        public EpisodeFileResolver() 
        {
        
        }

        public string SeriesFolder { get; init; }



        public IEnumerable<EpisodeFile> GetAllFiles(DirectoryInfo di)
        {
            foreach (FileInfo file in di.GetFiles())
                yield return new EpisodeFile { FileInfo = file};
            foreach(DirectoryInfo childDi in di.GetDirectories())
                foreach(EpisodeFile childFile in GetAllFiles(childDi))
                    yield return childFile;
        }


        public async ValueTask<bool> PullFilesAndNames()
        {

            if (SeriesFolder != null)
            {
                DirectoryInfo di = new DirectoryInfo(SeriesFolder);
                if(di.Exists)
                {
                    EpisodeFile[] files = GetAllFiles(di).ToArray();

                    string fullBaseDirName = di.FullName;
                    foreach(EpisodeFile file in files)
                    {
                        file.EstimateEpisode(fullBaseDirName);
                    }
                }
            }
            return false;
        }



    }
}
