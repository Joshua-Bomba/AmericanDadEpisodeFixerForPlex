
using AmericanDadEpisodeFixerForPlex;
using Microsoft.Extensions.Configuration;

var builder = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("episodefixer.appsettings.json").AddUserSecrets<EpisodeMetaDataHandler>();

IConfiguration config = builder.Build();




await using (EpisodeMetaDataHandler epfix = new EpisodeMetaDataHandler
{
    EpisodeDataEndpoint = config["EpisodeList"],
    CachePage = config["CachePage"],
    CachedEpisodes = config["CachedEpisodes"],
    LogOutputFile = config["LogOutputFile"]
})
{
    ValueTask<bool> processedSucessfully = epfix.ProcessEpisodeData();

    var extensions = config.GetSection("IncludedExtensions").GetChildren().Select(x => x.Value).ToHashSet();

    EpisodeFileHandler efr = new EpisodeFileHandler
    {
        SeriesFolder = config["SeriesFolder"],
        IncludedExtensions = extensions
    };
    if(await efr.PullFilesAndNames())
    {
        Console.WriteLine($"Got Episodes from {efr.SeriesFolder}");
        if (await processedSucessfully)
        {
            efr.SortEpisodesBasedonMetaData(epfix);
        }
        else
        {
            Console.WriteLine("Could not process episode metadata");
        }
    }
    else
    {
        Console.WriteLine($"Couple Not Process all the episodes from {efr.SeriesFolder}");
    }
    
}


