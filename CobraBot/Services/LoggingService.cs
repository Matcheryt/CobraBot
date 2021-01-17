using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Threading.Tasks;
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
        }

        /// <summary> Method for logging messages to the console. </summary>
        public static Task LogAsync(LogMessage message)
        {
            if (message.Exception is CommandException cmdException)
            {
                Console.WriteLine($"[Command/{message.Severity}] {cmdException.Command.Aliases.First()}"
                                  + $" failed to execute in {cmdException.Context.Channel}.");
                Console.WriteLine(cmdException);
            }
            else
                Console.WriteLine($"[General/{message.Severity}] {DateTime.Now:d} {message.Message}");

            return Task.CompletedTask;
        }
    }
}