using Discord.Commands;
using Discord;
using Discord.WebSocket;
using System;
using System.Reflection;
using System.Threading.Tasks;
using CobraBot.Helpers;
using Microsoft.Extensions.DependencyInjection;
using CobraBot.Handlers;

namespace CobraBot
{
    public class CommandHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly IServiceProvider _services;
        private readonly CommandService _commands;

        //Constructor
        public CommandHandler(IServiceProvider services)
        {
            _commands = services.GetRequiredService<CommandService>();
            _client = services.GetRequiredService<DiscordSocketClient>();
            _services = services;

            //Handle events
            _client.MessageReceived += HandleCommandAsync;
            _commands.Log += CommandLogging;
        }

        public async Task InitializeAsync()
        {
            await _commands.AddModulesAsync(
                assembly: Assembly.GetEntryAssembly(),
                services: _services);
        }

        private Task CommandLogging(LogMessage arg)
        {
            Console.WriteLine(DateTime.Now.Date.ToString("dd/MM/yyyy" + arg));
            return Task.CompletedTask;
        }

        //Called whenever a user sends a message
        private async Task HandleCommandAsync(SocketMessage rawMessage)
        {
            var msg = rawMessage as SocketUserMessage;

            //If msg == null or if the message was sent by another bot, then return
            if (msg == null || msg.Author.IsBot)
                return;

            int argPos = 0;

            var context = new SocketCommandContext(_client, msg);


            //Access saved prefixes
            string savedPrefix = DatabaseHandler.GetPrefix(context.Guild.Id);
            //Prefix to be used
            string prefix;

            //If there isn't a saved prefix for specified guild, then use default prefix
            if (savedPrefix == null)
            {
                prefix = "-";
                if (!msg.HasStringPrefix(prefix, ref argPos)) return;
            }
            //If there is a saved prefix, use it as the prefix
            else
            {
                prefix = savedPrefix;
                if (!msg.HasStringPrefix(prefix, ref argPos)) return;
            }


            //If the message is received on the bot's DM channel, then we ignore it
            //as we only want to process commands used on servers
            if (context.IsPrivate)
                return;

            //If the message received, has the command prefix, then we execute the command
            var result = await _commands.ExecuteAsync(context, argPos, _services);

            //If any errors happen while executing the command, they are handled here
            #region ErrorHandling
            if (result.Error != CommandError.UnknownCommand)
                //Prints to console whenever a user uses a command
                Console.WriteLine(DateTime.UtcNow.ToString("dd/MM/yy HH:mm:ss") + " Command     " + context.User + " has used the following command " + "'" + msg + "'" + " on server: " + context.Guild.Name);            

            if (!result.IsSuccess && result.Error != CommandError.UnknownCommand)
            {
                //Handle object not found command error
                if (result.Error == CommandError.ObjectNotFound)
                {
                    //If command is -usinfo
                    if (msg.Content.Contains("usinfo"))
                    {
                        await context.Channel.SendMessageAsync(embed: await Helper.CreateErrorEmbed("**User not found!**"));
                        return;
                    }

                    await context.Channel.SendMessageAsync(embed: await Helper.CreateErrorEmbed("**Object not found!**"));
                }

                if (result.Error == CommandError.UnmetPrecondition)
                {
                    await context.Channel.SendMessageAsync(embed: await Helper.CreateErrorEmbed("**No permission!**\n" + result.ErrorReason));
                }

                //Handle bad argument count command error
                if (result.Error == CommandError.BadArgCount)
                {
                    if (msg.Content.Contains("setbotgame"))
                        return;

                    await context.Channel.SendMessageAsync(embed: await Helper.CreateErrorEmbed("**Missing Arguments!** Please check command syntax -help"));
                }
                
                if (result.Error == CommandError.Exception)
                {
                    await context.Channel.SendMessageAsync(embed: await Helper.CreateErrorEmbed("An error occurred, please report it to Matcher#0183\n" + result.ErrorReason));
                }

                //Handle parse failed command error
                if (result.Error == CommandError.ParseFailed)
                {
                    await context.Channel.SendMessageAsync(embed: await Helper.CreateErrorEmbed("**Parse Failed!** Please check command syntax"));
                    return;
                }
            }
            //If there are not errors but the command is unknown, send message to server that the command is unknown
            else if (result.Error == CommandError.UnknownCommand)
            {
                await context.Channel.SendMessageAsync(embed: await Helper.CreateErrorEmbed($"**Unknown Command:** Type {prefix}help to see available commands."));
            }
            #endregion
        }
    }
}