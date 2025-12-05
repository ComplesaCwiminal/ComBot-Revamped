
using ComBot_Revamped.Commands;

namespace ComBot_Revamped
{

    public class ExitCommand : Command
    {
        public override string[] Names => new string[] { "exit", "quit", "bye", "goodbye", "stop" };
        public override Dictionary<string, string> Arguments => new Dictionary<string, string>();
        public override string Description => "Exits the program.";

        public override async Task Run(CancellationToken ct, params string[] args)
        {
            Environment.Exit(0);
        }

        public override void OnExit()
        {
            Console.WriteLine("Bye!");
        }
    }
}