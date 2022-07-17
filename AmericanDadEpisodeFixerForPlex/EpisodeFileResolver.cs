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


        private bool ProcessSeasonAndEpisode(string season, string episode)
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
            return seasonValid && episodeValid;
        }

        public bool EstimateEpisode(string baseDir)
        {
            string name = FileInfo.FullName;
            string season;
            string episode;
            string fName;
            bool valid = false;
            try
            {

                name = name.Replace(baseDir, string.Empty);
                season = Regex.Match(name, FIND_SEASON, RegexOptions.IgnoreCase).Value;
                episode = Regex.Match(name, FIND_EPISODE, RegexOptions.IgnoreCase).Value;

                valid = ProcessSeasonAndEpisode(season, episode);

                if (Season != null && EpisodeNumber == null)
                {
                    fName = FileInfo.Name;
                    episode = Regex.Match(fName, "(?<="+ Season.Value +") ?\\d{1,2}").Value;
                    valid = ProcessSeasonAndEpisode(season,episode);
                    if(EpisodeNumber == null)
                    {
                        episode = Regex.Match(fName, "\\d{1,2}").Value;
                    }
                    valid = ProcessSeasonAndEpisode(season, episode);
                }
                else if(season == null && EpisodeNumber == null)
                {
                    //did not run into this scenerio
                }

            }
            catch
            {
                return false;
            }
            return valid;
        }

    }

    public class Episodes : List<EpisodeFile>
    {
        private DirectoryInfo _dir;
        public Episodes(DirectoryInfo dir)
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


    public class EpisodeFileResolver
    {
        private Episodes episodes;
        public EpisodeFileResolver() 
        {
        
        }

        public string SeriesFolder { get; init; }


        public HashSet<string> IncludedExtensions { get; init; }



        public void GetAllFiles(DirectoryInfo di, Episodes episodes)
        {
            foreach (FileInfo file in di.GetFiles())
                if(IncludedExtensions.Contains(file.Extension.TrimStart('.').ToUpper()))
                    episodes.Add(new EpisodeFile { FileInfo = file});
            foreach(DirectoryInfo childDi in di.GetDirectories())
            {
                GetAllFiles(childDi, episodes);
            }

        }


        public async ValueTask<bool> PullFilesAndNames()
        {

            if (SeriesFolder != null)
            {
                DirectoryInfo di = new DirectoryInfo(SeriesFolder);
                if(di.Exists)
                {
                    episodes = new Episodes(di);
                    GetAllFiles(di, episodes);
                    return episodes.ProcessEpisodes();
                }
            }
            return false;
        }



    }
}
