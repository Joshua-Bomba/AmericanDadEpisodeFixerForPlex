using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmericanDadEpisodeFixerForPlex
{
    public class EpisodeFixer : IAsyncDisposable
    {

        private HttpClient _client;
        private Task _beforeFinish;
        public EpisodeFixer() 
        {
            _client = new HttpClient();
        }

        public string EpisodeDataEndpoint { get; init; }

        public string? CachePage { get; init; }

        private async ValueTask<string?> GetPageData()
        {
            if(CachePage != null)
            {
                if (File.Exists(CachePage))
                {
                    return await File.ReadAllTextAsync(CachePage);
                }
            }
            var response = await _client.GetAsync(EpisodeDataEndpoint);

            if (response.IsSuccessStatusCode)
            {
                 string content = await response.Content.ReadAsStringAsync();
                if(CachePage != null)
                {
                    _beforeFinish = File.WriteAllTextAsync(CachePage, content);
                }
                return content;
            }
            else
            {
                return null;
            }
        }


        public async ValueTask<bool> ProcessEpisodeData()
        {
            string? content = await GetPageData();
            if(!string.IsNullOrWhiteSpace(content))
            {
                //would like to pull from plex since it's better but i'm not sure how there api works
                //and this is supposto be quick so i'm gonna pull from wikipea and handle it my own way
                return true;
            }
            else
            {
                return false;
            }
        }

        public async ValueTask DisposeAsync()
        {
            if(_beforeFinish != null)
            {
                await _beforeFinish;
            }
        }
    }
}
