﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AmericanDadEpisodeFixerForPlex
{

    public class EpisodeFileHandler
    {

        public EpisodeFileHandler() 
        {
        
        }

        public string SeriesFolder { get; init; }

        public EpisodeFiles Episodes { get; set; }


        public HashSet<string> IncludedExtensions { get; init; }

        public string? OutputFolder { get; init; }


        public void SetOutputPaths()
        {
            Episodes.CalculateMoves(OutputFolder);
        }


        private void GetAllFiles(DirectoryInfo di, EpisodeFiles episodes)
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
                    Episodes = new EpisodeFiles(di);
                    GetAllFiles(di, Episodes);
                    return Episodes.EstimateEpisodes();
                }
            }
            return false;
        }


        private Dictionary<int,List<EpisodeFile>> GetEpisodesForEachSeason()
        {
            return Episodes.GroupBy(x => x.Season.Value).ToDictionary(x => x.Key, y => y.ToList());
        }

        private bool AssociateEpisode(Dictionary<int, EpisodeMetaData> propertySeasonOrder,EpisodeFile myEpisode)
        {
            //get our episode number based on the offset
            int episodeNumber = myEpisode.GetNewEpisodeNumber();
            if (propertySeasonOrder.ContainsKey(episodeNumber))
            {
                myEpisode.AssociatedEpisode = propertySeasonOrder[episodeNumber];
                propertySeasonOrder.Remove(episodeNumber);
                return true;
            }
            return false;
        }

        private List<EpisodeFile> HandleOverFlow(Dictionary<int, EpisodeMetaData> propertySeasonOrder, List<EpisodeFile> overFlow,int lastEpisode,ref int startOffSet)
        {
            List<EpisodeFile> newOverFlow = new List<EpisodeFile>();
            foreach (EpisodeFile ef in overFlow)//go though our overflow episodes
            {
                if (AssociateEpisode(propertySeasonOrder, ef))
                {
                    int newEpisodeNumber = ef.GetNewEpisodeNumber();
                    if (startOffSet < newEpisodeNumber)
                        startOffSet = newEpisodeNumber;
                }
                else
                {
                    ef.OverFlowOffSet += lastEpisode;
                    newOverFlow.Add(ef);

                }
            }
            return newOverFlow;
        }

        public void SortEpisodesBasedonMetaData(EpisodeMetaDataHandler emd)
        {
            //We will get the proper order of the show
            Dictionary<int, Dictionary<int, EpisodeMetaData>> properOrder  = emd.GetEpisodesForEachSeason();
            
            //we will get our order
            Dictionary<int, List<EpisodeFile>> myOrder = this.GetEpisodesForEachSeason();

            //will get the lowest season we have
            int min = myOrder.Keys.Min();
            //will get he highest season we have 
            int max = myOrder.Keys.Max();

            //We need to do this in order because will need to overflow episodes from one season to the next
            //hang tight
            List<EpisodeFile>? nextSeasonFlowOver = null;
            for (int i = min;i <= max || (nextSeasonFlowOver != null &&nextSeasonFlowOver.Any());i++)
            {
                
                
                
                List<EpisodeFile> myEpisodes;//will grab our list of episodes
                int currentSeasonLastEpisode = 0;
                int lastEpisodeLastSeason = 0;
                if (myOrder.ContainsKey(i))
                {
                    myEpisodes = myOrder[i];
                }
                else if(properOrder.ContainsKey(i))
                {
                    //if we don't have any but our proper order has some
                    //then will just use an empty collection
                    //this is so we have overflow stuff from the last season into this one if required
                    myEpisodes = new List<EpisodeFile>();
                }
                else
                {
                    //these seasons don't exist
                    continue;
                }
                
                //grab the proper order of the episodes
                Dictionary<int, EpisodeMetaData> propertySeasonOrder = properOrder[i];

                //will find out what is the last episode
                currentSeasonLastEpisode = propertySeasonOrder.Max(x => x.Key);

                //if we are overflowing we need to know how much to offset this seasons episodes
                int seasonStartOffSet = 0;

                if (nextSeasonFlowOver == null)
                {
                    nextSeasonFlowOver = new List<EpisodeFile>();
                }
                else if (nextSeasonFlowOver.Any())
                {                
                    //we need to handle overflowing last season into this one
                    nextSeasonFlowOver = HandleOverFlow(propertySeasonOrder,nextSeasonFlowOver, lastEpisodeLastSeason, ref seasonStartOffSet);
                }

                lastEpisodeLastSeason = currentSeasonLastEpisode;


                foreach (EpisodeFile episode in myEpisodes)
                {
                    episode.OverFlowOffSet -= seasonStartOffSet;
                    if (!AssociateEpisode(propertySeasonOrder, episode))
                    {
                        episode.OverFlowOffSet += currentSeasonLastEpisode;
                        nextSeasonFlowOver.Add(episode);
                    }
                }
            }                
        }
    }
}
