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
    internal class SendAudioToConnected : Command
    {

        public override string[] Names { get; } = { "joinvoicechat", "joinvc", "entervc" };

        public override Dictionary<string, string> Arguments => new Dictionary<string, string> { { "guild", "GUILD" }, { "channel", "CHANNEL" } };

        public override string Description { get; } = "Enters a voice chat and awaits further instructions";

        public override async Task Run(CancellationToken ct, params string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Error: Too few arguments.");
                return;
            }
            
            try {
                var vc = await VcManager.ConnectToChannel(ulong.Parse(args[1]));

                await vc.StartAsync();

                ct.Register(async () =>
                {
                    await Program.client.UpdateVoiceStateAsync(new VoiceStateProperties(vc.GuildId, null)); 
                });
            } catch(ArgumentException ae)
            {
                Console.WriteLine("Channel Invalid!");
            }
        }
    }
}