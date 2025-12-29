#if DEBUG
[assembly: System.Reflection.Metadata.MetadataUpdateHandlerAttribute(typeof(ComBot_Revamped.Commands.CommandHotReloadManager))]

namespace ComBot_Revamped.Commands
{
    public class CommandHotReloadManager
    {
        public static event Action<Type[]?>? UpdateApplicationEvent;

        internal static void ClearCache(Type[]? types)
        {
            // We don't have a use for this, but I got this from a snippet
            // It's also not unlikely this'll be useful in the future
            Console.WriteLine("Hot Reload Service event - ClearCache");
        }

        internal static void UpdateApplication(Type[]? types)
        {
            UpdateApplicationEvent?.Invoke(types);
 
            Console.WriteLine("Hot Reload Service event - UpdateApplication");

            // Reregistration needs to occur
            CommandManager.UpdateCommands(true);
        }
    }
}
#endif
