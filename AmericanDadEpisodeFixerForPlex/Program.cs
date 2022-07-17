
const string EPISODE_LIST_DATA = "https://en.wikipedia.org/wiki/List_of_American_Dad!_episodes";


HttpClient client = new HttpClient();

//would like to pull from plex since it's better but i'm not sure how there api works
//and this is supposto be quick so i'm gonna pull from wikipea and handle it my own way
var response = await client.GetAsync(EPISODE_LIST_DATA);

if(response.IsSuccessStatusCode)
{

}
else
{
    Console.WriteLine("Could not pull episode list data")
}
