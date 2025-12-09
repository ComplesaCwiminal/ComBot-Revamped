using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace ComBot_Revamped.Commands
{
    internal static class CommandManager
    {

        private static Action? onInit;
        private static Action? onConsoleReady;
        private static Action? onPostRegister;
        private static Action? onExit;
        private static Action? onInterruption;

        public static Dictionary<string, Command> CommandList { get; private set; } = new();
        public static Dictionary<Type, Command> CommandTypeLookup { get; private set; } = new();

        internal static Dictionary<Task, Command> _runningCommands = new();

        // Public way to read back the running commands as a snapshot in time;
        public static ReadOnlyDictionary<Task, Command> runningCommands {
            get {
                lock (_runningCommands)
                {
                    return new ReadOnlyDictionary<Task, Command>(_runningCommands);
                }
            }
        }

        static CancellationTokenSource cts = new();

        public static void Init()
        {
            RegisterCommands();
            if (onInit != null)
            {
                onInit.Invoke();
            }
        }

        public static void ConsoleReady()
        {
            if (onConsoleReady != null)
            {
                onConsoleReady.Invoke();
            }
        }
        
        public static void OnExit()
        {
            if (onExit != null)
            {
                onExit.Invoke();
            }
        }

        public static void RegisterCommands()
        {
            // This gets every type in the entire program, and loops through them.
            foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
            {
                if (type.IsSubclassOf(typeof(Command)))
                {
                    var cmd = Activator.CreateInstance(type) as Command;

                    // Skip duplicates
                    if (CommandList.ContainsValue(cmd))
                    {
                        continue;
                    }

                    // Register all it's aliases; Not just one
                    foreach (var alias in cmd.Names)
                    {
                        CommandList[alias.ToLower()] = cmd;
                    }

                    // Register it's type for funsies (for the web interface)
                    CommandTypeLookup[type] = cmd;


                    // Hook the command into the action system.
                    onExit += cmd.OnExit;
                    onInterruption += cmd.OnInterruption;
                    onInit += cmd.Init;
                    onConsoleReady += cmd.ConsoleReady;
                    onPostRegister += cmd.PostRegister;

                    cmd.OnRegister();
                }
            }

            if (onPostRegister != null)
            {
                onPostRegister.Invoke();
            }
        }
        
        public static Command? GetCommand<T>()
        {
            return CommandTypeLookup.ContainsKey(typeof(T)) ? CommandTypeLookup[typeof(T)] : null
                ;
        }

        public static Command? GetCommand(string name)
        {
            return CommandList.ContainsKey(name) ? CommandList[name] : null;
        }

        public static Task? StartCommand(Command? command, params string[] args)
        {

            if (command != null)
            {
                var tsk = command.Run(cts.Token, args);
                // We lock the running commands object itself because we only care that two operations aren't happening at once to it specifically
                lock (_runningCommands)
                {
                    _runningCommands.Add(tsk, command);
                }

                tsk.ContinueWith(t => {
                    // While addition is synchronous removal is not, so make sure two can't remove at once
                    lock (_runningCommands)
                    {
                        _runningCommands.Remove(t);
                    }
                });

                return tsk;
            }

            return null;
        }

        public static Task? StartCommand(string name, params string[] args)
        {
            var command = GetCommand(name);

            return StartCommand(command, args);
        }

        public static Task? StartCommand<T>(params string[] args)
        {
            var command = GetCommand<T>();

            var modArgs = args.ToList();

            modArgs.Insert(0, command != null ? command.Names[0] : "");

            return StartCommand(command, modArgs.ToArray());
        }

        public static void InterruptCommands()
        {
            cts.Cancel();

            foreach (var cmd in runningCommands)
            {
                cmd.Value.OnInterruption();
            }
        }
    }
}
