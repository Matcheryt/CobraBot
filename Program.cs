using System;
using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using CobraBot.Services;

namespace CobraBot
{
    public class Program
    {
        static void Main(string[] args)
        => new Program().StartAsync().GetAwaiter().GetResult();

        private string developToken;
        private string publishToken;

        private Configuration config = new Configuration();

        //Constructor initializing token strings from config file
        public Program()
        {
            //Bot tokens (you can delete developToken, I use it to switch between my hosted bot and development bot)
            //You can find the developToken and publishToken values in Bot.config file
            developToken = config.ReturnSavedValue("Tokens", "Develop");
            publishToken = config.ReturnSavedValue("Tokens", "Publish");
        }


        private DiscordSocketClient _client;
        public IServiceProvider services;
        private MusicService musicService;

        private CommandHandler _handler;

        public async Task StartAsync()
        {            
            _client = new DiscordSocketClient();

            musicService = new MusicService();

            //Handle events
            _client.Log += Log;
            _client.UserVoiceStateUpdated += _client_UserVoiceStateUpdated;
            _client.Ready += _client_Ready;

            //Login with developToken or publishToken
            await _client.LoginAsync(TokenType.Bot, developToken);

            await _client.StartAsync();

            services = new ServiceCollection()
                .AddSingleton(musicService)
                .BuildServiceProvider();

            //Constructor
            _handler = new CommandHandler(_client, services);    

            await Task.Delay(-1);
        }

        //Call CheckIfAlone method in MusicService whenever a user enters/leaves a voice channel
        private async Task _client_UserVoiceStateUpdated(SocketUser user, SocketVoiceState stateOld, SocketVoiceState stateNew)
        {
            await musicService.CheckIfAlone(user, stateOld, stateNew);
        }

        //Defines bot game when it starts
        private async Task _client_Ready()
        {
            //Change following string to change bot "Playing" status on discord
            string game = "CobraBot | -help";
            await _client.SetGameAsync(game);
            Console.WriteLine("---------------------Version 3.0---------------------");
            Console.WriteLine("'" + game + "'" + " has been defined as bot's currently playing 'game'");
        }

        //Error logging
        private Task Log(LogMessage arg)
        {
            Console.WriteLine(arg);
            return Task.CompletedTask;
        }
    }
}