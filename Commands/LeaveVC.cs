using ComBot_Revamped.Commands;
using ComBot_Revamped.Voice;
using NetCord;
using NetCord.Gateway;
using NetCord.Gateway.Voice;
using NetCord.Rest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComBot_Revamped.Commands
{
    internal class LeaveVC : Command
    {

        public override string[] Names { get; } = { "leavevc" };

        public override Dictionary<string, string> Arguments => new Dictionary<string, string> { { "guild", "GUILD" } };

        public override string Description { get; } = "Leaves a voice channel";

        public override async Task Run(CancellationToken ct, params string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Error: Too few arguments.");
                return;
            }

            var ch = (await Program.restClient.GetChannelAsync(ulong.Parse(args[1]))) as IGuildChannel;

            await Program.client.UpdateVoiceStateAsync(new VoiceStateProperties(ch.GuildId, null));
        }
    }
}