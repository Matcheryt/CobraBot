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
using Interactivity;

namespace CobraBot
{
    public class Program
    {
        private static void Main(string[] args) => new Program().StartAsync().GetAwaiter().GetResult();

        private readonly DiscordSocketClient _client;
        private readonly CommandHandler _handler;
        
        private readonly LavaNode _lavaNode;
        private readonly MusicService _musicService;
        private readonly ModerationService _moderationService;

        //Constructor initializing token strings from config file and configuring services
        public Program()
        {
            //Configure services
            var services = ConfigureServices();
            _lavaNode = services.GetRequiredService<LavaNode>();
            _handler = services.GetRequiredService<CommandHandler>();
            _client = services.GetRequiredService<DiscordSocketClient>();
            _musicService = services.GetRequiredService<MusicService>();
            _moderationService = services.GetRequiredService<ModerationService>();
            services.GetRequiredService<LoggingService>();
        }

        public async Task StartAsync()
        {
            //Handle events
            _client.UserVoiceStateUpdated += _musicService.UserVoiceStateUpdated;
            _client.UserJoined += _moderationService.UserJoinedServer;
            _client.UserLeft += _moderationService.UserLeftServer;
            _client.Ready += _client_Ready;
            //_client.JoinedGuild += _client_JoinedGuild;

            //Login with developToken or publishToken
            await _client.LoginAsync(TokenType.Bot, Configuration.DevelopToken);

            await _client.StartAsync();

            await _handler.InitializeAsync(); 

            await Task.Delay(-1);
        }

        //Fired when the bot joins a new guild
        //private Task _client_JoinedGuild(SocketGuild guild)
        //{
        //    guild.DefaultChannel.SendMessageAsync(embed: await Helpers.Helper.CreateBasicEmbed("Hello, I'm Cobra!", "To get a list of commands, type ```"))
        //}

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
                .AddSingleton<InteractivityService>()
                .AddSingleton<CommandHandler>()
                .AddSingleton<LavaNode>()
                .AddSingleton(new LavaConfig
                {
                    LogSeverity = LogSeverity.Info
                })
                .AddDbContext<BotContext>()
                .AddMemoryCache()
                .AddSingleton<MusicService>()
                .AddSingleton<ModerationService>()
                .AddSingleton<ApiService>()
                .AddSingleton<FunService>()
                .AddSingleton<InfoService>()
                .AddSingleton<LoggingService>()
                .AddSingleton<MiscService>()
                .BuildServiceProvider();
        }

        //When bot is ready
        private async Task _client_Ready()
        {
            //Connect to Lavalink node if we don't have a connection
            if (!_lavaNode.IsConnected)
                await _lavaNode.ConnectAsync();

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