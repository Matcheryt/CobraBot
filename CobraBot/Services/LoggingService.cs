using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Threading.Tasks;
using Serilog;
using Victoria;

namespace CobraBot.Services
{
    public sealed class LoggingService
    {
        public LoggingService(LavaNode lavaNode)
        {
            //These tree already have log messages
            lavaNode.OnLog += LogAsync;
        }

        /// <summary> Method for logging messages to the console. </summary>
        public static Task LogAsync(LogMessage message)
        {
            Log.Information(message.ToString());

            return Task.CompletedTask;
        }
    }
}