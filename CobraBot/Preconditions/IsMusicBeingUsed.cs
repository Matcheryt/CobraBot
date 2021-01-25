/*
    Multi-purpose Discord Bot named Cobra
    Copyright (C) 2021 Telmo Duarte <contact@telmoduarte.me>

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License as published
    by the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Affero General Public License for more details.

    You should have received a copy of the GNU Affero General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>. 
*/

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
