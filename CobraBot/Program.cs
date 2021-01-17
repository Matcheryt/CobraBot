using System;
using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;
using CobraBot.Database;
using Microsoft.Extensions.DependencyInjection;
using CobraBot.Services;
using Victoria;
using Discord.Commands;
using CobraBot.Handlers;
using CobraBot.Services.Moderation;
using EFCoreSecondLevelCacheInterceptor;
using Interactivity;
using Microsoft.EntityFrameworkCore;
using SpotifyAPI.Web;

namespace CobraBot
{
    public class Program
    {
        private static void Main(string[] args) => new Program().StartAsync().GetAwaiter().GetResult();

        private readonly DiscordSocketClient _client;
        private readonly CommandHandler _handler;
        
        //Constructor initializing token strings from config file and configuring services
        public Program()
        {
            //Configure services
            var services = ConfigureServices();
            _handler = services.GetRequiredService<CommandHandler>();
            _client = services.GetRequiredService<DiscordSocketClient>();
            services.GetRequiredService<LoggingService>();
        }

        public async Task StartAsync()
        {
            //Handle events
            _client.Ready += Client_Ready;

            //Login with developToken or publishToken
            await _client.LoginAsync(TokenType.Bot, Configuration.DevelopToken);

            await _client.StartAsync();

            await _handler.InitializeAsync(); 

            await Task.Delay(-1);
        }

        private static ServiceProvider ConfigureServices()
        {
            return new ServiceCollection()
                .AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
                    {
                        MessageCacheSize = 100,
                        AlwaysDownloadUsers = true,
                        LogLevel = LogSeverity.Info,
                        ExclusiveBulkDelete = true
                    }))
                .AddSingleton(new CommandService(new CommandServiceConfig
                    {
                        DefaultRunMode = RunMode.Async,
                        CaseSensitiveCommands = false
                    }))
                .AddSingleton<CommandHandler>()
                .AddSingleton(new SpotifyClient(SpotifyClientConfig
                    .CreateDefault()
                    .WithAuthenticator(new ClientCredentialsAuthenticator(
                        Configuration.SpotifyClientId, 
                        Configuration.SpotifyClientSecret))
                ))
                .AddLogging()
                .AddLavaNode(x =>
                {
                    x.LogSeverity = LogSeverity.Info;
                })
                .AddMemoryCache()
                .AddEFSecondLevelCache(options =>
                {
                    options.UseMemoryCacheProvider().DisableLogging();
                    options.CacheAllQueries(CacheExpirationMode.Sliding, TimeSpan.FromHours(24));
                })
                .AddDbContextPool<BotContext>((services, options) =>
                {
                    options.UseSqlite("Data Source=CobraDB.db");
                    options.AddInterceptors(services.GetRequiredService<SecondLevelCacheInterceptor>());
                })
                .AddSingleton<InteractivityService>()
                .AddSingleton<MusicService>()
                .AddSingleton<ModerationService>()
                .AddSingleton<LookupService>()
                .AddSingleton<ApiService>()
                .AddSingleton<FunService>()
                .AddSingleton<NsfwService>()
                .AddSingleton<InfoService>()
                .AddSingleton<LoggingService>()
                .AddSingleton<UtilitiesService>()
                .AddSingleton<SetupService>()
                .BuildServiceProvider();
        }

        //When bot is ready
        private async Task Client_Ready()
        {
            //Following instruction sets bot "Playing" status
            const string game = "-help";
            await _client.SetGameAsync($"{game}", null, ActivityType.Listening);

            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine($@"
   ____      _                 ____        _   
  / ___|___ | |__  _ __ __ _  | __ )  ___ | |_ 
 | |   / _ \| '_ \| '__/ _` | |  _ \ / _ \| __|
 | |__| (_) | |_) | | | (_| | | |_) | (_) | |_ 
  \____\___/|_.__/|_|  \__,_| |____/ \___/ \__|
                      
                         Version {System.Reflection.Assembly.GetEntryAssembly()?.GetName().Version?.ToString(2)}
");
            Console.ResetColor();
            Console.WriteLine("'" + game + "'" + " has been defined as Cobra's currently playing 'game'");
            Console.WriteLine($"I'm now online on {_client.Guilds.Count} guilds\n");
        }
    }
}