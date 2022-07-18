using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmericanDadEpisodeFixerForPlex
{
    public class EpisodeMover
    {
        private Dictionary<string, EpisodeFile> _moveInstructions;
        public EpisodeMover(){}

        public string ConfigFile { get; init; }

        public bool RevertMode { get; init; }

        public string FailureLog { get; init; }


        public async ValueTask LoadInFile()
        {
            string res = await File.ReadAllTextAsync(ConfigFile);
            _moveInstructions = JsonConvert.DeserializeObject<Dictionary<string, EpisodeFile>>(res);

        }

        private void SetToFrom(EpisodeFile moveInstruction,out FileInfo from,out FileInfo to)
        {
            from = RevertMode ? new FileInfo(moveInstruction.NewFile) : new FileInfo(moveInstruction.OriginalFile);
            to = RevertMode ? new FileInfo(moveInstruction.OriginalFile) : new FileInfo(moveInstruction.NewFile);
        }

        public void PerformMoves()
        {
            FileInfo from;
            FileInfo to;
            Stack<EpisodeFile> operationStack = new Stack<EpisodeFile>();
            try
            {
                Console.WriteLine("Started Moving Files");
                foreach (var moveInstruction in _moveInstructions)
                {
                    SetToFrom(moveInstruction.Value,out from,out to);
                    if (!to.Directory.Exists)
                    {
                        to.Directory.Create();
                    }
                    from.MoveTo(to.FullName, false);
                    operationStack.Push(moveInstruction.Value);
                }
                Console.WriteLine("Finished Moving Files");
            }
            catch
            {
                Console.WriteLine("Failed to move. Moving items back to where they were");
                //Undo
                StringBuilder? sb = null;
                while (operationStack.Any())
                {
                    EpisodeFile ep = operationStack.Pop();
                    SetToFrom(ep,out to,out from);//if we reverse the params it will move stuff the other way
                    try
                    {
                        from.MoveTo(to.FullName, false);
                    }
                    catch(Exception ex)
                    {
                        if(sb == null)
                        {
                            sb = new StringBuilder();
                            sb.AppendLine("We Encounter a failure(s) which we were not able to revert");
                        }
                        sb.Append("Failed to Move \"");
                        sb.AppendLine(from.FullName);
                        sb.Append("\" back to \"");
                        sb.Append(to.FullName);
                        sb.Append("\" Reason: ");
                        sb.AppendLine(ex.ToString());
                        Console.WriteLine("Failed To Move Back");
                    }

                }
                if(sb != null)
                {
                    string res = sb.ToString();
                    Console.Write(res);
                    try
                    {
                        File.WriteAllText(FailureLog, res);
                    }
                    catch 
                    {
                        Console.WriteLine("Failed to log the failure that occured during hanlding another failure. God help us all");
                    }
                }
                Console.WriteLine("Logging Failure Compleate. Continuing on with the Failure");
                throw;
            }
            
        }


    }
}
