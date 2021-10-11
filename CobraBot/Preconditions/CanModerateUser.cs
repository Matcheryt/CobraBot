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
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace CobraBot.Preconditions
{
    /// <summary>
    ///     Precondition used to check if the user that invoked the command has permission to moderate the specified
    ///     user.
    /// </summary>
    public class CanModerateUser : ParameterPreconditionAttribute
    {
        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context,
            ParameterInfo parameter, object value, IServiceProvider services)
        {
            if (value is not IGuildUser user)
                return PreconditionResult.FromError("Invalid user!");

            if (user.Id == context.User.Id)
                return PreconditionResult.FromError("You can't use this command on yourself!");

            if (user.GuildPermissions.Administrator)
                return PreconditionResult.FromError("The user you're trying to moderate is a mod/admin.");

            var bot = await context.Guild.GetCurrentUserAsync();

            if (bot is SocketGuildUser socketBot && user is SocketGuildUser socketUser)
                if (socketUser.Hierarchy > socketBot.Hierarchy)
                    return PreconditionResult.FromError(
                        "Cobra's role isn't high enough to moderate specified user. Move 'Cobra' role up above other roles.");

            return PreconditionResult.FromSuccess();
        }
    }
}