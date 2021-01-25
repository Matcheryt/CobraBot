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

namespace CobraBot.Modules
{
    [RequireContext(ContextType.Guild)]
    [RequireUserPermission(GuildPermission.Administrator)]
    [Name("Setup")]
    public class SetupModule : ModuleBase<SocketCommandContext>
    {
        public SetupService SetupService { get; set; }

        [Command("setup")]
        [Name("Setup"), Summary("Starts bot setup process.")]
        public async Task Setup()
            => await SetupService.SetupAsync(Context);

        #region Prefix, Welcome Channel, Role on Join, Moderation Channel
        [Command("prefix")]
        [Name("Prefix"), Summary("Changes bot prefix for current server.")]
        public async Task SetPrefix(string prefix)
            => await ReplyAsync(embed: await SetupService.ChangePrefixAsync(prefix, Context));

        //Set welcome channel
        [Command("setwelcome")]
        [Name("Set welcome channel"), Summary("Sets channel where join/left messages are shown.")]
        public async Task SetWelcomeChannel(ITextChannel textChannel)
            => await ReplyAsync(embed: await SetupService.SetWelcomeChannel(textChannel));

        //Reset welcome channel
        [Command("resetwelcome")]
        [Name("Reset welcome channel"), Summary("Resets channel where join/left messages are shown.")]
        public async Task ResetWelcomeChannel()
            => await ReplyAsync(embed: await SetupService.ResetWelcomeChannel(Context));

        //Set role on join
        [Command("setroleonjoin")]
        [Name("Set role on join"), Summary("Sets default role that users receive when they join the server.")]
        public async Task SetRoleOnJoin(IRole role)
            => await ReplyAsync(embed: await SetupService.SetRoleOnJoin(Context.Guild, role));

        //Reset role on join
        [Command("resetroleonjoin")]
        [Name("Reset role on join"), Summary("Resets role that users receive when they join the server.")]
        public async Task ResetRoleOnJoin()
            => await ReplyAsync(embed: await SetupService.ResetRoleOnJoin(Context));


        //Reset role on join
        [Command("pc toggle")]
        [Name("Toggle private chat"), Summary("Enables/disables private chat. (WIP feature)")]
        public async Task TogglePc()
            => await SetupService.EnablePrivateChatAsync(Context);

        #endregion
    }
}
