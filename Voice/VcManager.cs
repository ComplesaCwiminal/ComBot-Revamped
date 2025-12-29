using NetCord;
using NetCord.Gateway;
using NetCord.Gateway.Voice;
using NetCord.Rest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComBot_Revamped.Voice
{
    public static class VcManager
    {
        static Dictionary<ulong, VoiceClient> voiceConnections = new();
        static Dictionary<ulong, Stream> AudioOutStreams = new();

        public static async Task<VoiceClient?> ConnectToChannel(ulong channelID)
        {
            var voiceChannel = await Program.restClient.GetChannelAsync(channelID) as IGuildChannel;

            if(voiceChannel == null)
            {
                throw new ArgumentException("channelID does not point to a valid Channel!");
            }

            if (voiceConnections.ContainsKey(voiceChannel.GuildId))
            {
                throw new Exception("Connected to two voice channels in the same guild! You need to disconnect from one first!");
            }

            var vClient = await Program.client.JoinVoiceChannelAsync(voiceChannel.GuildId, voiceChannel.Id);

            voiceConnections[voiceChannel.GuildId] = vClient;

            vClient.Disconnect += new Func<DisconnectEventArgs, ValueTask>(async (arg) =>
            {
                if (!arg.Reconnect)
                {
                    Console.WriteLine("Client Disconnected!");
                    voiceConnections.Remove(vClient.GuildId);
                    AudioOutStreams.Remove(vClient.GuildId);
                }
            });
            return vClient;
        }

        public static Stream? GetOrCreateVCStream(ulong guildID)
        {
            if(voiceConnections.ContainsKey(guildID))
            {
                // If the stream exists then just return it
                if(AudioOutStreams.ContainsKey(guildID))
                {
                    return AudioOutStreams[guildID];
                }else
                {
                    // Otherwise grab the connnection and create a stream, to add it to the dict.
                    var con = voiceConnections[guildID];
                    var outStream = con.CreateOutputStream();

                    AudioOutStreams.Add(guildID, outStream);

                    // and just return the stream
                    return outStream;
                }
            }else
            {
                Console.WriteLine("You need a voice channel to make a stream!");
                return null;
            }
        }

        public static VoiceClient? TryGetVoiceConnection(ulong guildID)
        {
            if(voiceConnections.ContainsKey(guildID))
            {
                return voiceConnections[guildID];
            }
            return null;
        }

        public static void PreDisconnect()
        {
            // Flush and dispose of all the audio streams
            foreach(Stream str in AudioOutStreams.Values)
            {
                str.Flush();
                str.Dispose();
            }
            foreach (VoiceClient client in voiceConnections.Values)
            {
                client.CloseAsync();
                Program.client.UpdateVoiceStateAsync(new VoiceStateProperties(client.GuildId,null));
                client.Dispose();
            }

            // Dispose of all the references by clearing out the dict.
            AudioOutStreams = new Dictionary<ulong, Stream>();
            voiceConnections = new Dictionary<ulong, VoiceClient>();
        }
    }
}
