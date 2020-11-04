using Discord.Commands;
using Discord;
using Discord.WebSocket;
using System;
using System.Reflection;
using System.Threading.Tasks;
using CobraBot.Helpers;
using Microsoft.Extensions.DependencyInjection;

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
            Console.WriteLine(arg);
            return Task.CompletedTask;
        }

        //Called whenever a user sends a message
        private async Task HandleCommandAsync(SocketMessage rawMessage)
        {
            var msg = rawMessage as SocketUserMessage;

            if (msg == null)
                return;

            //Set prefix
            //If you want a string to be a prefix, uncomment the next line
            //string prefix = "string_prefix_here" // <- declare a string prefix
            char prefix = '-'; //<- declare char prefix

            int argPos = 0;

            var context = new SocketCommandContext(_client, msg);


            //We check if the message received has the prefix we set. If it doesn't,
            //then return, as we don't want to process the message as a command
            //Change 'HasCharPrefix' to 'HasStringPrefix' if you want the prefix to be a string
            if (!msg.HasCharPrefix(prefix, ref argPos)) return;

            //If the message received, has the command prefix, then we execute the command
            var result = await _commands.ExecuteAsync(context, argPos, _services);

            //If any errors happen while executing the command, they are handled here
            #region ErrorHandling
            if (result.Error != CommandError.UnknownCommand)
                //Prints to console whenever a user uses a command
                Console.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " Command     " + context.User + " has used the following command " + "'" + msg + "'" + " on server: " + context.Guild.Name);

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
                    return;
                }

                if (result.Error == CommandError.UnmetPrecondition)
                {
                    //If command is -clean
                    if (msg.Content.Contains("clean"))
                    {
                        await context.Channel.SendMessageAsync(embed: await Helper.CreateErrorEmbed("**No permission!** You need to have 'Manage Messages' permission"));
                        return;
                    }
                }

                //Handle bad argument count command error
                if (result.Error == CommandError.BadArgCount)
                {
                    if (msg.Content.Contains("setbotgame"))
                        return;

                    await context.Channel.SendMessageAsync(embed: await Helper.CreateErrorEmbed("**Missing Arguments!** Please check command syntax -help"));
                    return;
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
                await context.Channel.SendMessageAsync(embed: await Helper.CreateErrorEmbed("**Unknown Command:** Type -help to see available commands."));
            }
            #endregion
        }
    }
}