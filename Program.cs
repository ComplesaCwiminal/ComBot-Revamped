using ComBot_Revamped.Commands;
using ComBot_Revamped.Servers;
using ComBot_Revamped.Voice;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NetCord;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using NetCord.Hosting.Rest;
using NetCord.Logging;
using NetCord.Rest;
using System;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Text.Unicode;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml.Serialization;
using static NetCord.Rest.GuildMessageSearchResult;

namespace ComBot_Revamped
{
    static internal class Program
    {
        public static string[] arguments { get; internal set; }
        static string configFile = "Config.ini";
        // The amount of iterations for our pkbdf2
        static int hashIterations = 16384;

        // Used to make swapping how I choose to make password input easier.
        static Func<int, Config?, (string?, Config?)>? passwordProvider;

        public static GatewayClient client;
        public static RestClient restClient;

        public static CancellationTokenSource disconnectCanceller = new();

        static bool firstConnect = true;

        static async Task Main(string[] args)
        {
            arguments = args;
            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;

            StyleUtils.resetStyle();

            var botRunner = Task.Run(() => { Program.RunBotAsync(args).GetAwaiter().GetResult(); });
            await botRunner;
        }

        private static void CurrentDomain_ProcessExit(object? sender, EventArgs e)
        {
            VcManager.PreDisconnect();
            CommandManager.OnExit();
        }

        private static (string?, Config?) passwordFallback(int state, Config cfg)
        {
            switch (state) {
                case 1:
                    if (cfg.requirePassword.Value)
                    {
                        Console.WriteLine("Enter Bot Password: ");
                        Console.Write("> ");
                        return (Console.ReadLine(), null);
                    }
                break;
                    case 0:
                        Console.WriteLine("Enter Bot token: ");
                        Console.Write("> ");
                        var inp = Console.ReadLine();
                        var token = Encoding.Default.GetBytes(inp);
                        Console.WriteLine("Enter Bot password: ");
                        Console.Write("> ");
                        var password = Console.ReadLine();

                        var config = new Config();

                        if (password != null && password.Length > 0)
                        {
                            var salt = RandomNumberGenerator.GetBytes(64);

                            var pwBytes = Rfc2898DeriveBytes.Pbkdf2(password, salt, hashIterations, HashAlgorithmName.SHA512, 32);

                            config.password = Convert.ToBase64String(pwBytes);

                            config.salt = Convert.ToBase64String(salt);

                            config.requirePassword = true;

                            var nonce = RandomNumberGenerator.GetBytes(12);
                            config.nonce = Convert.ToBase64String(nonce);

                            byte[] bytes = new byte[token.Length];
                            byte[] tag = new byte[16];

                            new ChaCha20Poly1305(SHA256.HashData(Encoding.Default.GetBytes(password))).Encrypt(nonce, token, bytes, tag);

                            config.token = Convert.ToBase64String(bytes);
                            config.tag = Convert.ToBase64String(tag);
                        } else
                        {
                            config.token = Encoding.Default.GetString(token);
                            config.requirePassword = false;
                        }

                        config.hash = Convert.ToBase64String(SHA512.HashData(JsonSerializer.SerializeToUtf8Bytes(config)));

                        var fh = File.Create(configFile);
                        var json = JsonSerializer.SerializeToUtf8Bytes(config);

                        fh.Write(json);

                        fh.Close();

                        return (password, JsonSerializer.Deserialize<Config>(json, new JsonSerializerOptions
                        {
                            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
                        }));

                    break;
                default:
                        Console.WriteLine("Unknown State");
                    return (null, null);
                break;
            }
            return (null, null);
        }

        // Run this as a thread. It's infinitely blocking.
        static async Task CommandHandler(CancellationToken disconnectionToken)
        {
            while (!disconnectionToken.IsCancellationRequested)
            {
                Console.Write("\r> ");
                string input = Console.ReadLine() ?? "exit";
                string[] CommandSplit = input.Split(' ');

                // Commands are tasks and each run on their own thread.
                Task? commandTask = CommandManager.RunCommand(CommandSplit[0].ToLower(), CommandSplit);

                // It'll be null if no command was found
                if (commandTask == null)
                {
                    Console.WriteLine(string.Format("{0} is not a recognized command", CommandSplit[0]));
                }else
                {
                    try
                    {
                        await commandTask; // In this scenario we don't want multiple tasks running at once.
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine(string.Format("{0} command ran into an unhandled exception! ({1})", CommandSplit[0], ex.GetType().Name));
                        Console.WriteLine(string.Format("Details are as follows: \n {0}", ex));
                    }
                }
            }
        }

        static async Task RunBotAsync(string[] args)
        {
            string? result = null;
            Config? configObj = null;


            int input = File.Exists(configFile) ? 1 : 0;

            if (input == 1)
            {
                var fileContent = File.ReadAllText(configFile, Encoding.UTF8);
                Console.WriteLine(fileContent);
                configObj = JsonSerializer.Deserialize<Config>(fileContent, new JsonSerializerOptions
                {
                    Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
                });
            }

            var success = false;
            string? token = null;
            var attempts = 0;

            while (!success && attempts < 5)
            {
                Config conf;
                if (passwordProvider != null)
                {
                    (result, conf) = passwordProvider.Invoke(input, configObj);
                } else
                {
                    (result, conf) = passwordFallback(input, configObj);
                }

                configObj = conf == null ? configObj : conf;

                    (success, token) = TryBotLogin(configObj, result ?? "");

                attempts++;
            }

            if (success)
            {
                var pageTask = Task.Run(() => WebApp.Run(arguments));

                CommandManager.Init();

                var builder = Host.CreateApplicationBuilder();

                builder.Services.AddDiscordGateway(options =>
                {
                    options.Intents = GatewayIntents.All;
                    options.Token = token;

                }).AddDiscordRest(options =>
                {
                    options.Token = token;
                });

                var host = builder.Build();

                client = host.Services.GetService(typeof(GatewayClient)) as GatewayClient;
                restClient = host.Services.GetService(typeof(RestClient)) as RestClient;

                client.Ready += ClientReady;

                await host.StartAsync();


                token = null;

                await Task.Delay(-1);
            }
            else
            {
                throw new Exception("Invalid Login!");
            }
        }

        static async private ValueTask ClientReady(ReadyEventArgs args)
        {
            if (firstConnect)
            {
                firstConnect = false;
                StyleUtils.resetStyle();
                // Don't await this. It's infinite.
                var commandHandler = Task.Run(() => CommandHandler(disconnectCanceller.Token));
            }
        }

        public static (bool success, string token) TryBotLogin(Config cfg, string password)
        {
            // Check if it's missing it's value to default, if not check it's actual value is false.
            if (!cfg.requirePassword.HasValue || cfg.requirePassword.Value)
            {
                // Precheck to ensure no garbage data.
                if (cfg.salt == null || cfg.token == null || cfg.password == null || cfg.hash == null || cfg.nonce == null || cfg.tag == null)
                {
                    return (false, "");
                }

                var hash = Convert.FromBase64String(cfg.hash);
                cfg.hash = null;

                var shhhh = SHA512.HashData(JsonSerializer.SerializeToUtf8Bytes(cfg));

                if (!CryptographicOperations.FixedTimeEquals(shhhh, hash))
                {
                    throw new IOException("Hash Invalid! Corruption likely");
                }

                // Doesn't actually matter which encoding I use as long as it's internally consistent
                var sltBytes = Convert.FromBase64String(cfg.salt);
                var pwBytes = Convert.FromBase64String(cfg.password);
                var nonceBytes = Convert.FromBase64String(cfg.nonce);
                var tokenCipherBytes = Convert.FromBase64String(cfg.token);
                var cipherTagBytes = Convert.FromBase64String(cfg.tag);
                
                // Potentially consider playing with the iteration number or the hash algorithm.
                // I left the password as a string as if byte[] conversion isn't constant time, then you could figure out the password length externally
                // However our salt's size isn't a secret.
                var passwordHash = (Rfc2898DeriveBytes.Pbkdf2(password, sltBytes, hashIterations, HashAlgorithmName.SHA512, 32));

                if (CryptographicOperations.FixedTimeEquals(passwordHash, pwBytes))
                {
                    // I'm debating how I plan on dealing with the fact the other route is faster. Research is needed ig.

                    var output = new byte[tokenCipherBytes.Length];

                    new ChaCha20Poly1305(SHA256.HashData(Encoding.Default.GetBytes(password))).Decrypt(nonceBytes, tokenCipherBytes, cipherTagBytes, output);

                    return (true, Encoding.Default.GetString(output));
                }

                 return (false, null);
            }

            return (true, cfg.token);
        }
    }
}