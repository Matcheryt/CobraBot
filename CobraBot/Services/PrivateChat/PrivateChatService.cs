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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CobraBot.Common.EmbedFormats;
using CobraBot.Database;
using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;

namespace CobraBot.Services.PrivateChat
{
    public sealed class PrivateChatService
    {
        private readonly BotContext _botContext;
        private readonly DiscordSocketClient _client;

        public PrivateChatService(DiscordSocketClient client, BotContext botContext)
        {
            _botContext = botContext;
            _client = client;

            client.UserVoiceStateUpdated += UserVoiceStateUpdated;
        }


        /// <summary>Fired whenever someone joins/leaves a voice channel.
        /// <para>Automatically deletes the private voice channel if the channel has no users in it.</para>
        /// </summary>
        private async Task UserVoiceStateUpdated(SocketUser user, SocketVoiceState oldState, SocketVoiceState newState)
        {
            if (user.IsBot)
                return;

            if (oldState.VoiceChannel == null)
                return;

            if (oldState.VoiceChannel == newState.VoiceChannel)
                return;

            //Check if the channel is on the private chat database
            var privateChat =  await _botContext.PrivateChats.AsQueryable().FirstOrDefaultAsync(x => x.ChannelId == oldState.VoiceChannel.Id);

            if (privateChat == null)
                return;

            //OldState voice channel is the channel id we want
            var channelId = privateChat.ChannelId;

            //We check if the old voice state
            if (!(newState.VoiceChannel?.Id != channelId || newState.VoiceChannel == null))
                return;

            //We count every user in the channel that isn't a bot, and put that result in 'users' variable
            int users = oldState.VoiceChannel.Users.Count(u => !u.IsBot);

            //If there are no users left in the voice channel, we make the bot leave
            if (users < 1)
            {
                var guild = await _client.Rest.GetGuildAsync(privateChat.GuildId);
                
                var channelToDelete = await guild.GetVoiceChannelAsync(channelId);

                if (channelToDelete == null)
                    return;

                await channelToDelete.DeleteAsync(new RequestOptions { AuditLogReason = "Delete private channel as it is empty" });

                _botContext.PrivateChats.Remove(privateChat);
                await _botContext.SaveChangesAsync();
            }
        }


        /// <summary> Method for creating a new voice channel. </summary>
        /// <param name="context"> The command context. </param>
        /// <param name="allowedUsers"> The users allowed to join the channel. If null, the channel will be public. </param>
        public async Task CreateChannelAsync(SocketCommandContext context, IUser[] allowedUsers)
        {
            if (context.User is not IGuildUser user)
                return;

            //Check if the user is in a voice channel when he casts the comand
            if (user.VoiceChannel is null)
            {
                await context.Channel.SendMessageAsync(
                    embed: CustomFormats.CreateErrorEmbed("You must be in a voice channel first!"));
                return;
            }

            //Retrieve guild settings
            var guildSettings = await _botContext.Guilds.AsQueryable().AsNoTracking().FirstOrDefaultAsync(x => x.GuildId == user.Guild.Id);

            if (guildSettings is null)
                return;

            if (guildSettings.PrivChannelsCategory == 0)
                return;

            var privateChat = await _botContext.PrivateChats.AsQueryable().AsNoTracking().FirstOrDefaultAsync(x => x.UserId == context.User.Id);

            //Check if the user already has an active voice channel
            if (privateChat != null)
            {
                await context.Channel.SendMessageAsync(embed: CustomFormats.CreateErrorEmbed("You already have an active channel!\nUse the `pc delete` command to delete it."));
                return;
            }

            //List of overwrite permissions
            var permissionsList = new List<Overwrite>
            {
                //We add the channel owner permissions
                new(context.User.Id, PermissionTarget.User, new OverwritePermissions(
                    viewChannel: PermValue.Allow, 
                    connect: PermValue.Allow, 
                    useVoiceActivation: PermValue.Allow,
                    muteMembers: PermValue.Allow,
                    manageChannel: PermValue.Allow, 
                    manageRoles: PermValue.Allow)),
            };

            if (allowedUsers.Length > 0)
            {
                //Add connect, view and voice activation permissions to every allowed user
                foreach (var allowedUser in allowedUsers)
                {
                    permissionsList.Add(new Overwrite(allowedUser.Id, PermissionTarget.User, new OverwritePermissions(
                        viewChannel: PermValue.Allow, 
                        connect: PermValue.Allow, 
                        useVoiceActivation: PermValue.Allow)));
                }

                //Deny permission for everyone except the allowed users
                permissionsList.Add(new Overwrite(context.Guild.EveryoneRole.Id, PermissionTarget.Role,
                    new OverwritePermissions(viewChannel: PermValue.Deny)));
            }
            else
            {
                //If no allowed users are passed, then the channel is created as public
                permissionsList.Add(new Overwrite(context.Guild.EveryoneRole.Id, PermissionTarget.Role, new OverwritePermissions(viewChannel: PermValue.Allow, connect: PermValue.Allow)));
            }

            RestVoiceChannel createdChannel;

            try
            {
                //Create the channel with specified permissions
                createdChannel = await context.Guild.CreateVoiceChannelAsync($"{context.User.Username}'s chat",
                    channelConfig =>
                    {
                        channelConfig.PermissionOverwrites = permissionsList;
                        channelConfig.CategoryId = guildSettings.PrivChannelsCategory;
                    },
                    new RequestOptions {AuditLogReason = "Create private chat"});
            }
            catch (Exception)
            {
                await context.Channel.SendMessageAsync(
                    embed: CustomFormats.CreateErrorEmbed("There has been an error"));
                return;
            }


            //Add private channel to the database
            await _botContext.PrivateChats.AddAsync(new Database.Models.PrivateChat(context.User.Id, createdChannel.Id, context.Guild.Id));
            await _botContext.SaveChangesAsync();

            await user.ModifyAsync(x => x.Channel = createdChannel);

            await context.Message.AddReactionAsync(new Emoji("👍"));
        }


        /// <summary> Method for deleting the command's invoker private channel. </summary>
        /// <param name="context"> The command context. </param>
        public async Task DeleteChannelAsync(SocketCommandContext context)
        {
            var privateChat = await _botContext.PrivateChats.AsQueryable().FirstOrDefaultAsync(x => x.UserId == context.User.Id);
            
            //Check if the user already has an active voice channel
            if (privateChat == null)
            {
                await context.Channel.SendMessageAsync(embed: CustomFormats.CreateErrorEmbed("You don't have an active channel!"));
                return;
            }

            var channelToDelete = context.Guild.GetVoiceChannel(privateChat.ChannelId);
            if (channelToDelete != null)
            {
                await channelToDelete.DeleteAsync(new RequestOptions
                    {AuditLogReason = "User deleted his private channel"});
            }

            _botContext.PrivateChats.Remove(privateChat);
            await _botContext.SaveChangesAsync();
                
            await context.Message.AddReactionAsync(new Emoji("👍"));
        }
    }
}
