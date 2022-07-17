
using AmericanDadEpisodeFixerForPlex;
using Microsoft.Extensions.Configuration;

var builder = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("episodefixer.appsettings.json").AddUserSecrets<EpisodeFixer>();

IConfiguration config = builder.Build();


//would like to pull from plex since it's better but i'm not sure how there api works
//and this is supposto be quick so i'm gonna pull from wikipea and handle it my own way


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
