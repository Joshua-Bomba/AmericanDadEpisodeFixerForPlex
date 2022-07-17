
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
    ValueTask<bool> processedSucessfully = epfix.ProcessEpisodeData();

    var extensions = config.GetSection("IncludedExtensions").GetChildren().Select(x => x.Value).ToHashSet();

    EpisodeFileResolver efr = new EpisodeFileResolver
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


