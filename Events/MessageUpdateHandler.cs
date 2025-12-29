using Microsoft.Extensions.Logging;

using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using NetCord.Rest;

namespace ComBot_Revamped.Events
{
    public class MessageUpdateHandler(ILogger<MessageCreationHandler> logger) : IMessageUpdateGatewayHandler
    {
        public ValueTask HandleAsync(Message message)
        {
            logger.LogInformation("{}", message.Content);
            return default;
        }
    }
}
