using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AmericanDadEpisodeFixerForPlex
{

    public class EpisodeFileResolver
    {
        private Episodes episodes;
        public EpisodeFileResolver() 
        {
        
        }

        public string SeriesFolder { get; init; }


        public HashSet<string> IncludedExtensions { get; init; }



        private void GetAllFiles(DirectoryInfo di, Episodes episodes)
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


        private Dictionary<int,List<EpisodeFile>> GetEpisodesForEachSeason()
        {
            return episodes.GroupBy(x => x.Season.Value).ToDictionary(x => x.Key, y => y.ToList());
        }

        private List<EpisodeFile> ProcessEpisodes(Dictionary<int, Episode> propertySeasonOrder,List<EpisodeFile> myEpisodes,int offset)
        {
            List<EpisodeFile> overFlow = new List<EpisodeFile>();
            foreach (EpisodeFile episode in myEpisodes)
            {
                if(!AssociateEpisode(propertySeasonOrder,episode.EpisodeNumber.Value - offset,episode))
                {
                    overFlow.Add(episode);
                }
            }
            return overFlow;
        }

        private bool AssociateEpisode(Dictionary<int, Episode> propertySeasonOrder,int episodeNumber,EpisodeFile myEpisode)
        {
            if (propertySeasonOrder.ContainsKey(episodeNumber))
            {
                myEpisode.AssociatedEpisode = propertySeasonOrder[episodeNumber];
                propertySeasonOrder.Remove(episodeNumber);
                return true;
            }
            return false;
        }

        public void SortEpisodesBasedonMetaData(EpisodeMetaData emd)
        {
            Dictionary<int, Dictionary<int, Episode>> properOrder  = emd.GetEpisodesForEachSeason();

            Dictionary<int, List<EpisodeFile>> myOrder = this.GetEpisodesForEachSeason();

            int min = myOrder.Keys.Min();
            int max = myOrder.Keys.Max();



            List<EpisodeFile>? nextSeasonFlowOver = null;
            int lastSeasonHighestEpisode = 0;
            for (int i = min;i <= max;i++)
            {
                List<EpisodeFile> myEpisodes = myOrder[i];
                Dictionary<int, Episode> propertySeasonOrder = properOrder[i];
                int offSet = 0;
                if(nextSeasonFlowOver != null&&nextSeasonFlowOver.Any())
                {
                    foreach (EpisodeFile ef in nextSeasonFlowOver)
                    {
                        int num = ef.EpisodeNumber.Value - lastSeasonHighestEpisode;
                        if (offSet < num)
                            offSet = num;

                        if (!AssociateEpisode(propertySeasonOrder, num, ef))
                        {
                            throw new DataMisalignedException();
                        }
                    }
                    nextSeasonFlowOver = null;
                }

                lastSeasonHighestEpisode = propertySeasonOrder.Max(x => x.Key);
                nextSeasonFlowOver = ProcessEpisodes(propertySeasonOrder, myEpisodes, offSet);

                if (propertySeasonOrder.Any())
                {
                    Console.Write("Could Not Find Episodes: ");
                    foreach(Episode episode in propertySeasonOrder.Values)
                    {
                        Console.Write(episode.CombinedEpisodeAndSeason());
                        Console.Write(" ");
                    }
                    Console.WriteLine();
                }
            }
        }
    }
}
