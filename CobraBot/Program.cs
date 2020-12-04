using System;
using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using CobraBot.Services;
using Victoria;
using Discord.Commands;
using CobraBot.Handlers;

namespace CobraBot
{
    public class Program
    {
        private static void Main(string[] args) => new Program().StartAsync().GetAwaiter().GetResult();

        private readonly DiscordSocketClient _client;
        private readonly ServiceProvider _services;
        private readonly LavaNode _lavaNode;
        private readonly CommandHandler _handler;

        private readonly MusicService _musicService;
        private readonly ModerationService _moderationService;
        private readonly ApiService _apiService;

        private readonly string developToken;
        private readonly string publishToken;

        //Constructor initializing token strings from config file and configuring services
        public Program()
        {
            //Bot tokens (you can delete developToken, I use it to switch between my hosted bot and development bot)
            //You can find the developToken and publishToken values in botconfig.json file
            developToken = Configuration.ReturnSavedValue("Tokens", "Develop");
            publishToken = Configuration.ReturnSavedValue("Tokens", "Publish");

            //Configure services
            _services = ConfigureServices();
            _lavaNode = _services.GetRequiredService<LavaNode>();
            _handler = _services.GetRequiredService<CommandHandler>();
            _client = _services.GetRequiredService<DiscordSocketClient>();
            _musicService = _services.GetRequiredService<MusicService>();
            _moderationService = _services.GetRequiredService<ModerationService>();
            _apiService = _services.GetRequiredService<ApiService>();
        }

        public async Task StartAsync()
        {            
            //Handle events
            _client.Log += Log;
            _client.UserVoiceStateUpdated += _musicService.UserVoiceStateUpdated;
            _client.UserJoined += _moderationService.UserJoinedServer;
            _client.UserLeft += _moderationService.UserLeftServer;
            _client.Ready += _client_Ready;
            //_client.JoinedGuild += _client_JoinedGuild;

            //Login with developToken or publishToken
            await _client.LoginAsync(TokenType.Bot, developToken);

            await _client.StartAsync();
           
            await _handler.InitializeAsync(); 

            await Task.Delay(-1);
        }

        //private Task _client_JoinedGuild(SocketGuild arg)
        //{
        //    arg.DefaultChannel.SendMessageAsync(embed: await Helpers.Helper.CreateBasicEmbed("Hello, I'm Cobra!", "To get a list of commands, type ```"))
        //}

        private static ServiceProvider ConfigureServices()
        {
            return new ServiceCollection()
                .AddSingleton<DiscordSocketClient>()
                .AddSingleton<CommandService>()
                .AddSingleton<CommandHandler>()
                .AddSingleton<LavaNode>()
                .AddSingleton(new LavaConfig())
                .AddSingleton<MusicService>()
                .AddSingleton<ModerationService>()
                .AddSingleton<ApiService>()
                .BuildServiceProvider();
        }

        //Defines bot game when it starts
        private async Task _client_Ready()
        {
            DatabaseHandler.Initialize();

            if (!_lavaNode.IsConnected)
            {
                //If bot restarts for some reason, this makes sure we have a clean LavaNode connection
                await _lavaNode.ConnectAsync();
            }

            //Change following string to change bot "Playing" status on discord
            const string game = "CobraBot | -help";
            await _client.SetGameAsync(game);

            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine(@"
   ____      _                 ____        _   
  / ___|___ | |__  _ __ __ _  | __ )  ___ | |_ 
 | |   / _ \| '_ \| '__/ _` | |  _ \ / _ \| __|
 | |__| (_) | |_) | | | (_| | | |_) | (_) | |_ 
  \____\___/|_.__/|_|  \__,_| |____/ \___/ \__|
                      
                         Version 4.3
");
            Console.ResetColor();
            Console.WriteLine("'" + game + "'" + " has been defined as bot's currently playing 'game'");
            Console.WriteLine($"I'm now online on {_client.Guilds.Count} guilds\n");
        }

        //Error logging
        private static Task Log(LogMessage arg)
        {           
            Console.WriteLine(DateTime.UtcNow.Date.ToString("dd/MM/yy") + " " + arg);
            return Task.CompletedTask;
        }
    }
}