using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmericanDadEpisodeFixerForPlex
{
    public class EpisodeFileResolver
    {
        public EpisodeFileResolver() 
        {
        
        }

        public string SeriesFolder { get; init; }



        public static IEnumerable<FileInfo> GetAllFiles(DirectoryInfo di)
        {
            foreach (FileInfo file in di.GetFiles())
                yield return file;
            foreach(DirectoryInfo childDi in di.GetDirectories())
                foreach(FileInfo childFile in GetAllFiles(childDi))
                    yield return childFile;
        }


        public async ValueTask<bool> PullFilesAndNames()
        {

            if (SeriesFolder != null)
            {
                DirectoryInfo di = new DirectoryInfo(SeriesFolder);
                if(di.Exists)
                {
                    FileInfo[] files = GetAllFiles(di).ToArray();
                }
            }
            return false;
        }



    }
}
