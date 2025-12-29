using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using NetCord.Hosting.Rest;

namespace ComBot_Revamped.Events
{
    public class ClientReady : IReadyGatewayHandler
    {
        public ValueTask HandleAsync(ReadyEventArgs arg)
        {
            StyleUtils.resetStyle();

            return default;
        }
    }
}
