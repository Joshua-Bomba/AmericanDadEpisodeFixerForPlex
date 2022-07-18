
using AmericanDadEpisodeFixerForPlex;
using Microsoft.Extensions.Configuration;

var builder = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("episodefixer.appsettings.json").AddUserSecrets<EpisodeMetaDataHandler>();

IConfiguration config = builder.Build();



if(bool.Parse(config["GenerateMoveInstructions:Enabled"]))
{

    await using (EpisodeMetaDataHandler epfix = new EpisodeMetaDataHandler
    {
        EpisodeDataEndpoint = config["GenerateMoveInstructions:EpisodeList"],
        CachePage = config["GenerateMoveInstructions:CachePage"],
        CachedEpisodes = config["GenerateMoveInstructions:CachedEpisodes"],
        OutputFile = config["GenerateMoveInstructions:OutputFile"]
    })
    {
        ValueTask<bool> processedSucessfully = epfix.ProcessEpisodeData();

        var extensions = config.GetSection("GenerateMoveInstructions:IncludedExtensions").GetChildren().Select(x => x.Value).ToHashSet();

        EpisodeFileHandler efr = new EpisodeFileHandler
        {
            SeriesFolder = config["GenerateMoveInstructions:SeriesFolder"],
            IncludedExtensions = extensions
        };
        if (await efr.PullFilesAndNames())
        {
            Console.WriteLine($"Got Episodes from {efr.SeriesFolder}");
            if (await processedSucessfully)
            {
                efr.SortEpisodesBasedonMetaData(epfix);
                efr.Episodes.CalculateMoves();
                epfix.OutputEpisodeChangeFile(efr.Episodes);
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
}




