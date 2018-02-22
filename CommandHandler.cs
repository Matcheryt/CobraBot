using Discord.Commands;
using Discord;
using Discord.WebSocket;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace CobraBot
{
    public class CommandHandler
    {
        private DiscordSocketClient _client;
        private IServiceProvider _servicecollection;

        private CommandService _service;

        public CommandHandler(DiscordSocketClient client, IServiceProvider services)
        {
            _client = client;
            _servicecollection = services;

            _service = new CommandService();

            _service.AddModulesAsync(Assembly.GetEntryAssembly());

            //Handle events
            _client.MessageReceived += HandleCommandAsync;
            _service.Log += CommandLogging;
        }

        private Task CommandLogging(LogMessage arg)
        {
            Console.WriteLine(arg);
            return Task.CompletedTask;
        }

        //Handle command
        private async Task HandleCommandAsync(SocketMessage s)
        {
            var msg = s as SocketUserMessage;

            if (msg == null)
                return;

            var context = new SocketCommandContext(_client, msg);

            //Set prefix (change it to string if you want a string prefix)
            char prefix = '-';

            int argPos = 0;

            //Change to HasStringPrefix if you want a string prefix
            if (msg.HasCharPrefix(prefix, ref argPos))
            {
                //Prints whenever a user uses a command
                Console.WriteLine(context.User + " usou o comando " + "'" + msg + "'" + " no servidor: " + context.Guild.Name);

                var result = await _service.ExecuteAsync(context, argPos, _servicecollection);
                var errorBuilder = new EmbedBuilder().WithColor(Color.Red);

                if (!result.IsSuccess && result.Error != CommandError.UnknownCommand)
                {
                    //Handle object not found command error
                    if (result.Error == CommandError.ObjectNotFound)
                    {
                        if (msg.Content.Contains("usinfo"))
                        {
                            errorBuilder.WithDescription("**User not found!**");
                            await context.Channel.SendMessageAsync("", false, errorBuilder.Build());
                            return;
                        }

                        errorBuilder.WithDescription("**Object not found!**");
                        await context.Channel.SendMessageAsync("", false, errorBuilder.Build());
                        return;
                    }

                    if (result.Error == CommandError.UnmetPrecondition)
                    {
                        if (msg.Content.Contains("clean"))
                        {
                            errorBuilder.WithDescription("**No permission!**");
                            await context.Channel.SendMessageAsync("", false, errorBuilder.Build());
                            return;
                        }
                    }

                    //Handle bad argument count command error
                    if (result.Error == CommandError.BadArgCount)
                    {  
                        errorBuilder.WithDescription("**Bad Argument Count!** Please check command syntax");
                        await context.Channel.SendMessageAsync("", false, errorBuilder.Build());
                        return;
                    }

                    //Handle parse failed command error
                    if (result.Error == CommandError.ParseFailed)
                    {
                        if (msg.Content.Contains("random"))
                        {
                            errorBuilder.WithDescription("**Value cannot be greater than 2147483647 or lesser than -2147483647**");
                            await context.Channel.SendMessageAsync("", false, errorBuilder.Build());
                            return;
                        }

                        errorBuilder.WithDescription("**Parse Failed!** Please check command syntax");
                        await context.Channel.SendMessageAsync("", false, errorBuilder.Build());
                        return;
                    }
                }
                //If there are not errors but the command is unknown, output to server that the command is unknown
                else if (result.Error == CommandError.UnknownCommand)
                {
                    EmbedBuilder unknownCommandBuilder = new EmbedBuilder();
                    unknownCommandBuilder.WithColor(Color.Red)
                        .WithDescription("**Unknown Command:** Type -help to show the available commands.");
                    await context.Channel.SendMessageAsync("", false, unknownCommandBuilder.Build());
                }
            }        
        }

    }
}