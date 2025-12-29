using ComBot_Revamped.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace combot.Commands
{
    internal class Help : Command
    {
        public override string[] Names => new string[] { "help", "?" };
        public override Dictionary<string, string> Arguments => new();
        public override string Description => "Shows this message.";
        string[][] command_help_table;
        int command_name_max_length = 0;

        public async override Task Run(CancellationToken ct, params string[] args)
        {
            Console.WriteLine("ComBot Command Help");
            foreach (var entry in command_help_table) {
                string output = entry[0];
                output = output.PadRight(command_name_max_length);
                output += entry[1];

                Console.WriteLine(output);
            }
        }
        public override void PostRegister()
        {
            base.OnRegister();

            command_name_max_length = 0;
            List<string[]> command_help_table_temp = new();

            foreach (var command in CommandManager.CommandTypeLookup.Values) {
                // Names and aliases
                var help_table_entry = new string[2];
                help_table_entry[0] = command.Names[0];

                for (int i = 1; i < command.Names.Length; i++) {
                    help_table_entry[0] += ", " + command.Names[i];
                }

                command_name_max_length = Math.Max(help_table_entry[0].Length, command_name_max_length);
                help_table_entry[1] = command.Description;
                command_help_table_temp.Add(help_table_entry);
            }

            command_name_max_length += 2;
            command_help_table = command_help_table_temp.ToArray();
        }
    }
}
