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

namespace CobraBot.TypeReaders
{
    public class ExtendedUserTypeReader : UserTypeReader<IUser>
    {
        public override async Task<TypeReaderResult> ReadAsync(ICommandContext context, string input,
            IServiceProvider services)
        {
            var typeReaderResult = await base.ReadAsync(context, input, services);
            if (typeReaderResult.IsSuccess)
                return typeReaderResult;

            if (!ulong.TryParse(input, out var parseResult) || context is not SocketCommandContext ctx)
                return TypeReaderResult.FromError(CommandError.ObjectNotFound, "Unable to find user!");

            var user = await ctx.Client.Rest.GetUserAsync(parseResult);

            return user != null
                ? TypeReaderResult.FromSuccess(user)
                : TypeReaderResult.FromError(CommandError.ObjectNotFound, "Unable to find user!");
        }
    }
}