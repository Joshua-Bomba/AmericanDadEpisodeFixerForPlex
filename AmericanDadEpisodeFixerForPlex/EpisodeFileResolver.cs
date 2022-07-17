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
        public const string FIND_SEASON = "(?<=(s(eason)? ?))\\d{1,2}";
        public const string FIND_EPISODE = "(?<=(e(pisode)? ?))\\d{1,2}";

        public FileInfo FileInfo { get; set; }


        private void ProcessSeasonAndEpisode(string season, string episode)
        {
            bool seasonValid = !string.IsNullOrWhiteSpace(season);
            bool episodeValid = !string.IsNullOrWhiteSpace(episode);
            if(EpisodeNumber == null&& episodeValid)
            {
                EpisodeNumber = Convert.ToInt32(episode);
            }
            if(Season == null&& seasonValid)
            {
                Season = Convert.ToInt32(season);
            }
        }

        public void EstimateEpisode(string baseDir)
        {
            string name = FileInfo.FullName;
            string season;
            string episode;
            string fName;
            try
            {

                name = name.Replace(baseDir, string.Empty);
                season = Regex.Match(name, FIND_SEASON, RegexOptions.IgnoreCase).Value;
                episode = Regex.Match(name, FIND_EPISODE, RegexOptions.IgnoreCase).Value;

                ProcessSeasonAndEpisode(season, episode);

                if (Season != null && EpisodeNumber == null)
                {
                    fName = FileInfo.Name;
                    episode = Regex.Match(fName, "(?<="+ Season.Value +") ?\\d{1,2}").Value;
                    ProcessSeasonAndEpisode(season,episode);
                    if(EpisodeNumber == null)
                    {
                        episode = Regex.Match(fName, "\\d{1,2}").Value;
                    }
                    ProcessSeasonAndEpisode(season, episode);
                }
                else if(season == null && EpisodeNumber == null)
                {

                }
            }
            catch
            {
                throw;
            }
            
        }

    }


    public class EpisodeFileResolver
    {
        public EpisodeFileResolver() 
        {
        
        }

        public string SeriesFolder { get; init; }


        public HashSet<string> IncludedExtensions { get; init; }



        public IEnumerable<EpisodeFile> GetAllFiles(DirectoryInfo di)
        {
            foreach (FileInfo file in di.GetFiles())
                if(IncludedExtensions.Contains(file.Extension.TrimStart('.').ToUpper()))
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
