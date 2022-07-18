using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmericanDadEpisodeFixerForPlex
{
    public class EpisodeMover
    {
        private Dictionary<string, EpisodeFile> _moveInstructions;
        public EpisodeMover(){}

        public string ConfigFile { get; init; }

        public bool RevertMode { get; init; }


        public async ValueTask LoadInFile()
        {
            string res = await File.ReadAllTextAsync(ConfigFile);
            _moveInstructions = JsonConvert.DeserializeObject<Dictionary<string, EpisodeFile>>(res);

        }
    }
}
