using System;
using System.Threading.Tasks;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using Victoria;

namespace CobraBot.Preconditions
{
    /// <summary>
    /// Checks if the text channel invoking the command is the
    /// same channel where the bot has it's lava node player
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false)]
    public sealed class IsMusicBeingUsed : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            //Get lava node
            var lavaNode = services.GetRequiredService<LavaNode>();
            lavaNode.TryGetPlayer(context.Guild, out var player);

            return player == null
                ? Task.FromResult(PreconditionResult.FromSuccess())
                : Task.FromResult(player.TextChannel != context.Channel
                    ? PreconditionResult.FromError("The bot is already being used on a different channel!")
                    : PreconditionResult.FromSuccess());
        }
    }
}
