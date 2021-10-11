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
using CobraBot.Preconditions;
using Discord.Commands;

namespace CobraBot.Common.Extensions
{
    public static class ExtensionMethods
    {
        public static async Task<bool> HasPermissionToExecute(this CommandInfo command, ICommandContext context,
            IServiceProvider services)
        {
            foreach (var precondition in command.Module.Preconditions)
            {
                if (precondition is Ratelimit or RequireNsfwAttribute)
                    continue;

                var canExecute = await precondition.CheckPermissionsAsync(context, command, services);

                if (!canExecute.IsSuccess)
                    return false;
            }

            foreach (var precondition in command.Preconditions)
            {
                if (precondition is Ratelimit or RequireNsfwAttribute)
                    continue;

                var canExecute = await precondition.CheckPermissionsAsync(context, command, services);

                if (!canExecute.IsSuccess)
                    return false;
            }

            return true;
        }
    }
}