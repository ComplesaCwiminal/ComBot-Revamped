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
    internal class VcTalk : Command
    {

        public override string[] Names { get; } = { "vctalk", "sendvoice" };

        public override Dictionary<string, string> Arguments => new Dictionary<string, string> { { "guild", "GUILD" } };

        public override string Description { get; } = "Sends audio of some variety in a vc";

        public override async Task Run(CancellationToken ct, params string[] args)
        {
            if (args.Length < 2)
            {
                throw new ArgumentNullException("Too few arguments");
            }

            VoiceClient? vClient = VcManager.TryGetVoiceConnection(ulong.Parse(args[1]));

            if (vClient != null)
            {
                await vClient.EnterSpeakingStateAsync(new SpeakingProperties(SpeakingFlags.Microphone));

                var str = VcManager.GetOrCreateVCStream(ulong.Parse(args[1]));
            }else
            {
                throw new ArgumentException("Voice Channel is not valid");
            }
        }
    }
}