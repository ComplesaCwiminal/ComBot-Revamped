using Microsoft.Extensions.Logging;

using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using NetCord.Rest;

namespace ComBot_Revamped.Events
{
    public class MessageCreationHandler(ILogger<MessageCreationHandler> logger) : IMessageCreateGatewayHandler
    {
        public ValueTask HandleAsync(Message message)
        {
            logger.LogInformation(message.Content);
            return default;
        }
    }
}
