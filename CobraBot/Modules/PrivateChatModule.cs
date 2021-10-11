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

using System.Runtime.InteropServices;
using System.Threading.Tasks;
using CobraBot.Preconditions;
using CobraBot.Services.PrivateChat;
using Discord;
using Discord.Commands;

namespace CobraBot.Modules
{
    [RequireContext(ContextType.Guild)]
    [Name("Private Chat")]
    [RequirePrivateChat]
    public class PrivateChatModule : ModuleBase<SocketCommandContext>
    {
        public PrivateChatService PrivateChatService { get; set; }

        //Command for creating private channels
        [Command("pc create")]
        [Alias("pcc")]
        [Summary("Creates a private voice channel. If you don't specify allowed users, the channel will be public." +
                 "\nTo specify allowed users, just mention them (example: @Cobra pc create @User1 @User2 @User3)" +
                 "\nYou can change channel permissions anytime through Discord's Edit Channel screen.")]
        public async Task CreateChannel([Name("allowed users")] [Optional] params IUser[] allowedUsers)
        {
            await PrivateChatService.CreateChannelAsync(Context, allowedUsers);
        }


        //Command for deleting private channels
        [Command("pc delete")]
        [Alias("pcd")]
        [Summary("Deletes your active private channel")]
        public async Task DeleteChannel()
        {
            await PrivateChatService.DeleteChannelAsync(Context);
        }
    }
}