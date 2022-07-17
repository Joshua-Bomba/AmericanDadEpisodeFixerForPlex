using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmericanDadEpisodeFixerForPlex
{
    public class EpisodeFixer
    {

        private HttpClient _client;
        public EpisodeFixer() 
        {
            _client = new HttpClient();
        }

        public string EpisodeDataEndpoint { get; init; }



        public async ValueTask<bool> ProcessEpisodeData()
        {
            var response = await _client.GetAsync(EpisodeDataEndpoint);

            if (response.IsSuccessStatusCode)
            {

                return true;
            }
            else
            {
                return false;
            }
        }

    }
}
