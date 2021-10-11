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

using System.Threading.Tasks;
using CobraBot.Services;
using Discord;
using Discord.Commands;

namespace CobraBot.Modules
{
    [RequireContext(ContextType.Guild)]
    [RequireUserPermission(GuildPermission.Administrator)]
    [Name("Setup")]
    public class SetupModule : ModuleBase<SocketCommandContext>
    {
        public SetupService SetupService { get; set; }

        [Command("setup")]
        [Name("Setup")]
        [Summary("Starts bot setup process.")]
        public async Task Setup()
        {
            await SetupService.SetupAsync(Context);
        }


        [Command("prefix")]
        [Name("Prefix")]
        [Summary("Changes bot prefix for current server.")]
        public async Task SetPrefix(string prefix)
        {
            await ReplyAsync(embed: await SetupService.ChangePrefix(Context, prefix));
        }
    }
}