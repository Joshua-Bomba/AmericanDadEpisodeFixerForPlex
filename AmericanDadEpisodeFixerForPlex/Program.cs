
using AmericanDadEpisodeFixerForPlex;
using Microsoft.Extensions.Configuration;

var builder = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("episodefixer.appsettings.json").AddUserSecrets<EpisodeFixer>();

IConfiguration config = builder.Build();




EpisodeFixer epfix = new EpisodeFixer 
{ 
    EpisodeDataEndpoint = config["EpisodeList"] 
};

bool processedSucessfully = await epfix.ProcessEpisodeData();
if(processedSucessfully)
{

}
else
{
    Console.WriteLine("Could not pull episode list data");
}
