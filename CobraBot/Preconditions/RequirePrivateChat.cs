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
using System.Linq;
using System.Threading.Tasks;
using CobraBot.Database;
using Discord.Commands;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CobraBot.Preconditions
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false)]
    public class RequirePrivateChat : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var botContext = services.GetRequiredService<BotContext>();

            var guildSettings = botContext.Guilds.AsNoTracking().FirstOrDefault(x => x.GuildId == context.Guild.Id);

            if (guildSettings is null)
                return Task.FromResult(PreconditionResult.FromError("Private chat is not enabled on this guild!"));

            var isPrivateChatEnabled = guildSettings.IsPrivateChatEnabled;

            return Task.FromResult(!isPrivateChatEnabled ? PreconditionResult.FromError("Private chat is not enabled on this guild!") : PreconditionResult.FromSuccess());
        }
    }
}