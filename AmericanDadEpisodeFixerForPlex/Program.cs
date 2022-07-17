
using AmericanDadEpisodeFixerForPlex;
using Microsoft.Extensions.Configuration;

var builder = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("episodefixer.appsettings.json").AddUserSecrets<EpisodeMetaData>();

IConfiguration config = builder.Build();




await using (EpisodeMetaData epfix = new EpisodeMetaData
{
    EpisodeDataEndpoint = config["EpisodeList"],
    CachePage = config["CachePage"],
    CachedEpisodes = config["CachedEpisodes"]
})
{
    bool processedSucessfully = await epfix.ProcessEpisodeData();
    if (processedSucessfully)
    {

    }
    else
    {
        Console.WriteLine("Could not process episode data");
    }
}


