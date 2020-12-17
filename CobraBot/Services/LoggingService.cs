using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Victoria;

namespace CobraBot.Services
{
    public sealed class LoggingService
    {
        public LoggingService(DiscordSocketClient client, CommandService commandService, LavaNode lavaNode)
        {
            //These tree already have log messages
            client.Log += LogAsync;
            commandService.Log += LogAsync;
            lavaNode.OnLog += LogAsync;
            
            //Command executed fires when a command is executed
            commandService.CommandExecuted += OnCommandExecuted;
        }

        private static Task LogAsync(LogMessage message)
        {
            if (message.Exception is CommandException cmdException)
            {
                Console.WriteLine($"[Command/{message.Severity}] {cmdException.Command.Aliases.First()}"
                                  + $" failed to execute in {cmdException.Context.Channel}.");
                Console.WriteLine(cmdException);
            }
            else
                Console.WriteLine($"[General/{message.Severity}] {DateTime.Now:d} {message}");

            return Task.CompletedTask;
        }

        //Create new log message according to which command was executed and on which server
        private static async Task OnCommandExecuted(Optional<CommandInfo> command, ICommandContext context, IResult result)
            => await LogAsync(new LogMessage(LogSeverity.Info, "Command",
            $"{context.User} has used {context.Message} on {context.Guild}."));
    }
}