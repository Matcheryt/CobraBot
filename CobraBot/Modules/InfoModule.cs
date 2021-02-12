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

using CobraBot.Services;
using Discord;
using Discord.Commands;
using System.Threading.Tasks;
using CobraBot.Preconditions;

namespace CobraBot.Modules
{
    [RequireContext(ContextType.Guild)]
    [Name("Information"), Ratelimit(1, 1, Measure.Seconds)]
    public class InfoModule : ModuleBase<SocketCommandContext>
    {
        public InfoService InfoService { get; set; }

        //Sends a message with server information
        [Command("svinfo"), Alias("serverinfo")]
        [Name("Server info"), Summary("Shows current server info.")]
        public async Task ServerInfo()
            => await InfoService.ServerInfoAsync(Context);


        //Shows discord user info
        [Command("user"), Alias("whois", "usinfo", "userinfo")]
        [Name("User info"), Summary("Shows discord info for specified user.")]
        public async Task GetInfo(IUser user)
            => await ReplyAsync(embed: InfoService.ShowUserInfoAsync(user));


        //Shows help
        [Command("help")]
        [Name("Help"), Summary("Shows help command.")]
        public async Task Help()
            => await InfoService.HelpAsync(Context);


        //Shows help
        [Command("chelp")]
        [Name("Command Help"), Summary("Shows information about a command.")]
        public async Task Help([Remainder] string command)
            => await InfoService.HelpAsync(Context, command);


        //Shows uptime
        [Command("botinfo"), Alias("info", "binfo", "cobra", "uptime")]
        [Name("Bot Info"), Summary("Shows Cobra's information.")]
        public async Task BotInfo()
            => await InfoService.GetBotInfoAsync(Context);


        //Shows latency
        [Command("ping"), Alias("latency")]
        [Name("Ping"), Summary("Shows Cobra's latency.")]
        public async Task Latency()
            => await InfoService.LatencyAsync(Context);


        //Shows invitation link
        [Command("invite")]
        [Name("Invite"), Summary("Shows a link to invite Cobra to your server.")]
        public async Task Invite()
            => await InfoService.InviteAsync(Context);
    }
}