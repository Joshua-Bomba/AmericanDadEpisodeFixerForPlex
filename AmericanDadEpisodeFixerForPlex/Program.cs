
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
                Console.WriteLine("Beging to Sort Base on Provide MetaData");
                efr.SortEpisodesBasedonMetaData(epfix);
                Console.WriteLine("Episodes Sorted Base on Provide MetaData");
                efr.Episodes.CalculateMoves();
                Console.WriteLine("Calculating Moves");
                epfix.OutputEpisodeChangeFile(efr.Episodes);
                Console.WriteLine("Exporting output File");
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
    Console.WriteLine("Move Instructions Generated");
}

if (bool.Parse(config["PerformMove:Enabled"]))
{
    EpisodeMover em = new EpisodeMover
    {
        ConfigFile = config["PerformMove:ConfigFile"],
        RevertMode = bool.Parse(config["PerformMove:RevertMode"]),
        FailureLog = config["PerformMove:FailureLog"]
    };
    await em.LoadInFile();
    em.PerformMoves();

}





