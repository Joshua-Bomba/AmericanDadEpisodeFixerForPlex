﻿using Newtonsoft.Json;
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
        public static readonly string INVALID_FILE_CHAR = new string(Path.GetInvalidFileNameChars().Concat(Path.GetInvalidPathChars()).ToHashSet().ToArray());
        public static readonly Regex BAD_FILE_CHECK = new Regex($"[{ Regex.Escape(INVALID_FILE_CHAR)}]");


        public const string FIND_SEASON = "(?<=(s(eason)? ?))\\d{1,2}";
        public const string FIND_EPISODE = "(?<=(e(pisode)? ?))\\d{1,2}";
        [JsonIgnore]
        public FileInfo FileInfo { get; set; }

        public EpisodeMetaData AssociatedEpisode { get; set; }

        private string? _originalFile; 
        public string OriginalFile { get => _originalFile ??= FileInfo.FullName; set => _originalFile = value; }

        public string? NewFile { get; set; }


        [JsonIgnore]
        public int OverFlowOffSet { get; set; } = 0;

        public int GetNewEpisodeNumber() => EpisodeNumber.Value - OverFlowOffSet;


        private bool ProcessSeasonAndEpisode(string season, string episode)
        {
            bool seasonValid = !string.IsNullOrWhiteSpace(season);
            bool episodeValid = !string.IsNullOrWhiteSpace(episode);
            if (EpisodeNumber == null && episodeValid)
            {
                EpisodeNumber = Convert.ToInt32(episode);
            }
            if (Season == null && seasonValid)
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
                    episode = Regex.Match(fName, "(?<=" + Season.Value + ") ?\\d{1,2}").Value;
                    valid = ProcessSeasonAndEpisode(season, episode);
                    if (EpisodeNumber == null)
                    {
                        episode = Regex.Match(fName, "\\d{1,2}").Value;
                    }
                    valid = ProcessSeasonAndEpisode(season, episode);
                }
                else if (season == null && EpisodeNumber == null)
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

        public void CalculateMove(string dir)
        {
            string epName = BAD_FILE_CHECK.Replace(AssociatedEpisode.EpisodeName, "");
            string fName = string.Format("S{0:00}E{1:00} - {2}{3}", AssociatedEpisode.Season.Value, AssociatedEpisode.EpisodeNumber, epName, FileInfo.Extension);
            this.NewFile = Path.Combine(dir, $"Season {AssociatedEpisode.Season.Value}",fName);
        }

    }
}
