using ComBot_Revamped.Commands;
using NetCord.Rest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComBot_Revamped
{
    internal class Talk : Command
    {
        public override string[] Names => new string[]  {"talk","speak"};
        public override Dictionary<string, string> Arguments => new Dictionary<string, string> { {"channel", "CHANNEL" }, { "string", "Message to say in the channel" } };
        public override string Description => "Sends a message to the specified channel.";

        public override async Task Run(CancellationToken ct, params string[] args)
        {
            int i = 0;
            string text = "";
            if (args.Length >= 2)
            {
                i++;
            }
            foreach (string argument in args.Skip(2))
            {
                    if (text == "")
                    {
                        text += argument;
                    }
                    else
                    {
                        text += " " + argument;
                    }
            }
            try
            {
                    await Program.restClient.SendMessageAsync(ulong.Parse(args[1]), new MessageProperties
                    {
                        Content = text
                    });
            }
            catch (System.NullReferenceException e)
            {
                // did you ever notice how none of the argument things are the same?
                // yeah, me neither...
                Console.WriteLine("One or more arguments are missing from the command.", true);
                Console.WriteLine("Stacktrace is: " + e.StackTrace);
            }
            catch (System.FormatException e)
            {

                // Seriously tho
                Console.WriteLine("The channel ID failed to convert.", true);
                Console.WriteLine("Stacktrace is: " + e.StackTrace);
            }
        }
    }
}
