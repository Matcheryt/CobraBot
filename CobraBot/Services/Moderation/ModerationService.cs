#region License

/*CitizenEnforcer - Moderation and logging bot
Copyright(C) 2018-2020 Hawx
https://github.com/Hawxy/CitizenEnforcer

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published
by the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with this program.If not, see http://www.gnu.org/licenses/ */

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CobraBot.Common.EmbedFormats;
using CobraBot.Database;
using CobraBot.Database.Models;
using CobraBot.Helpers;
using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using Interactivity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace CobraBot.Services.Moderation
{
    public sealed class ModerationService
    {
        public enum CacheType
        {
            BanReject,
            UnbanReject
        }

        private readonly IMemoryCache _banCache;
        private readonly BotContext _botContext;
        private readonly InteractivityService _interactivityService;

        public ModerationService(BotContext botContext, InteractivityService interactivityService,
            DiscordSocketClient client, IMemoryCache memoryCache)
        {
            _botContext = botContext;
            _interactivityService = interactivityService;
            _banCache = memoryCache;

            //Events
            client.UserJoined += UserJoinedServer;
            client.UserLeft += UserLeftServer;
            client.UserBanned += UserBanned;
            client.UserUnbanned += UserUnbanned;
        }


        #region Update role

        /// <summary> Gives/removes role from specified user. </summary>
        /// <param name="user"> User to update role. </param>
        /// <param name="operation"> Operation (+ adds role, - removes role). </param>
        /// <param name="role"> Role to give/remove from user. </param>
        public static async Task<Embed> UpdateRoleAsync(IGuildUser user, char operation, IRole role)
        {
            //Get role which name equals role
            //var roleToUpdate = Helper.DoesRoleExist(user.Guild, role);

            switch (operation)
            {
                case '+':
                    await user.AddRoleAsync(role);
                    return CustomFormats.CreateBasicEmbed("Role added",
                        $"Role {role.Name} was successfully added to {user.Username}", 0x268618);

                case '-':
                    await user.RemoveRoleAsync(role);
                    return CustomFormats.CreateBasicEmbed("Role removed",
                        $"Role {role.Name} was successfully removed from {user.Username}", 0x268618);

                default:
                    return CustomFormats.CreateErrorEmbed(
                        "Invalid operation! Available operations are **+** (add) and **-** (remove).");
            }
        }

        #endregion


        public class CacheModel
        {
            public CacheModel(ulong guildId, CacheType cacheType = CacheType.BanReject)
            {
                GuildId = guildId;
                CacheType = cacheType;
            }

            public ulong GuildId { get; set; }
            public CacheType CacheType { get; set; }
        }

        #region User banned and unbanned

        /// <summary>
        ///     Fired whenever a user is banned from the server.
        /// </summary>
        private async Task UserBanned(SocketUser bannedUser, SocketGuild guild)
        {
            //Retrieve guild settings
            var guildSettings = _botContext.Guilds.AsNoTracking().FirstOrDefault(x => x.GuildId == guild.Id);

            if (guildSettings is null)
                return;

            //Check if guild has moderation channel enabled
            var moderationChannel = guild.GetTextChannel(guildSettings.ModerationChannel);
            if (moderationChannel == null)
                return;

            //Check if we have already sent a mod log by accessing the ban cache
            if (_banCache.TryGetValue(bannedUser.Id, out CacheModel value) && value.CacheType == CacheType.BanReject)
                return;

            //If we haven't sent a mod log already, set a cache entry for 5 seconds
            _banCache.Set(bannedUser.Id, new CacheModel(guild.Id), TimeSpan.FromSeconds(5));

            IEnumerable<RestAuditLogEntry> logs = null;

            try
            {
                logs = await guild.GetAuditLogsAsync(5).FlattenAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            var entry = logs?.FirstOrDefault(x => (x.Data as BanAuditLogData)?.Target.Id == bannedUser.Id);

            var caseId = await GenerateModCaseId(guild.Id);

            var modCase = entry != null
                ? new ModCase(entry.User, guild.Id, bannedUser, caseId, PunishmentType.Ban, entry.Reason)
                : new ModCase(guild.Id, bannedUser, caseId, PunishmentType.Ban);

            await _botContext.AddAsync(modCase);
            await _botContext.SaveChangesAsync();
            await SendModLog(moderationChannel, modCase);
        }

        /// <summary>
        ///     Fired whenever a user is unbanned from the server.
        /// </summary>
        private async Task UserUnbanned(SocketUser unbannedUser, SocketGuild guild)
        {
            //Retrieve guild settings
            var guildSettings = _botContext.Guilds.AsNoTracking().FirstOrDefault(x => x.GuildId == guild.Id);

            if (guildSettings is null)
                return;

            //Check if guild has moderation functionality enabled
            if (guildSettings.ModerationChannel == 0)
                return;

            IEnumerable<RestAuditLogEntry> logs = null;

            try
            {
                logs = await guild.GetAuditLogsAsync(5).FlattenAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            var entry = logs?.FirstOrDefault(x => (x.Data as UnbanAuditLogData)?.Target.Id == unbannedUser.Id);
            var moderationChannel = guild.GetTextChannel(guildSettings.ModerationChannel);
            await moderationChannel.SendMessageAsync(embed: ModerationFormats.UnbanEmbed(unbannedUser, entry?.User));
        }

        #endregion


        #region User join and left

        /// <summary>
        ///     Fired whenever someone joins the server.
        ///     <para>Used to log a message to a specific text channel.</para>
        /// </summary>
        public async Task UserJoinedServer(SocketGuildUser user)
        {
            var guild = user.Guild;

            //Retrieve guild settings
            var guildSettings = _botContext.Guilds.AsNoTracking().FirstOrDefault(x => x.GuildId == guild.Id);

            if (guildSettings is null)
                return;

            //Check if guild has moderation channel enabled
            var welcomeChannel = guild.GetTextChannel(guildSettings.WelcomeChannel);
            if (welcomeChannel == null)
                return;

            //Check if there is a valid role and give that role to the user
            if (guildSettings.RoleOnJoin != 0 &&
                Helper.DoesRoleExist(user.Guild, guildSettings.RoleOnJoin) is var role && role != null)
                await user.AddRoleAsync(role, new RequestOptions { AuditLogReason = "Auto role on join" });

            //Announce to WelcomeChannel that the user joined the server
            await welcomeChannel.SendMessageAsync(embed: CustomFormats.CreateBasicEmbed("User joined",
                $"{user} has joined the server!", Color.Green));
        }


        /// <summary>
        ///     Fired whenever someone leaves the server.
        ///     <para>Used to log a message to a specific text channel.</para>
        /// </summary>
        public async Task UserLeftServer(SocketGuildUser user)
        {
            var guild = user.Guild;

            //Retrieve guild settings
            var guildSettings = _botContext.Guilds.AsNoTracking().FirstOrDefault(x => x.GuildId == guild.Id);

            if (guildSettings is null)
                return;

            //Check if guild has moderation channel enabled
            var welcomeChannel = guild.GetTextChannel(guildSettings.WelcomeChannel);
            if (welcomeChannel == null)
                return;

            //If we do have a valid channel, announce that the user left the server
            await welcomeChannel.SendMessageAsync(
                embed: CustomFormats.CreateBasicEmbed("User left", $"{user} has left the server!", Color.DarkGrey));
        }

        #endregion


        #region Ban, Unban and Kick

        /// <summary>
        ///     Ban specified user from the server with reason.
        /// </summary>
        public async Task<Embed> BanAsync(IUser user, int pruneDays, string reason, SocketCommandContext context)
        {
            await context.Message.DeleteAsync();

            if (pruneDays < 0 || pruneDays > 7)
                return CustomFormats.CreateErrorEmbed("Prune days must be between 0 and 7");

            //Check if user is already banned
            var isBanned = await context.Guild.GetBanAsync(user);
            if (isBanned != null)
                return CustomFormats.CreateErrorEmbed($"{user.Username} is already banned!");

            await SendPunishmentDm(user, ModerationFormats.DmPunishmentEmbed("You have been banned!",
                $"You have been permanently banned from {context.Guild.Name}", context.Guild));

            _banCache.Set(user.Id, new CacheModel(context.Guild.Id), TimeSpan.FromSeconds(5));

            //Ban user
            await context.Guild.AddBanAsync(user, pruneDays, reason);

            var caseId = await GenerateModCaseId(context.Guild.Id);

            //Create modCase
            var modCase = new ModCase(context.User, context.Guild.Id, user, caseId, PunishmentType.Ban, reason);
            await _botContext.ModCases.AddAsync(modCase);
            await _botContext.SaveChangesAsync();
            await SendModLog(context.Guild, modCase);

            return ModerationFormats.CreateModerationEmbed(user, $"{user} banned",
                $"{user} was banned from the server for: {reason ?? "_No reason_"}.", Color.DarkGrey);
        }


        /// <summary>
        ///     Unbans specified user from the server.
        /// </summary>
        public async Task<Embed> UnbanAsync(IUser user, SocketCommandContext context)
        {
            await context.Message.DeleteAsync();

            var isBanned = await context.Guild.GetBanAsync(user);
            if (isBanned == null)
                return CustomFormats.CreateErrorEmbed($"{user} is not banned!");

            _banCache.Set(user.Id, new CacheModel(context.Guild.Id, CacheType.UnbanReject), TimeSpan.FromSeconds(5));

            await context.Guild.RemoveBanAsync(user);
            return CustomFormats.CreateBasicEmbed($"{user} unbanned", $"{user} was unbanned successfully.", 0x268618);
        }


        /// <summary>
        ///     Kick specified user from the server with reason.
        /// </summary>
        public async Task<Embed> KickAsync(IUser user, string reason, SocketCommandContext context)
        {
            await SendPunishmentDm(user, ModerationFormats.DmPunishmentEmbed("You have been kicked!",
                $"You have been kicked from {context.Guild.Name}", context.Guild));

            await ((IGuildUser)user).KickAsync(reason);

            var caseId = await GenerateModCaseId(context.Guild.Id);

            //Create mod case
            var modCase = new ModCase(context, user, caseId, PunishmentType.Kick, reason);
            await _botContext.ModCases.AddAsync(modCase);
            await _botContext.SaveChangesAsync();
            await SendModLog(context.Guild, modCase);

            return ModerationFormats.CreateModerationEmbed(user, $"{user} kicked",
                $"{user} was kicked from the server for: {reason ?? "_No reason_"}.", Color.DarkGrey);
        }

        #endregion


        #region Mute and Voice mute

        /// <summary>
        ///     Mutes specified user.
        ///     <para>Prevents the user from sending chat messages.</para>
        /// </summary>
        public async Task<Embed> MuteAsync(SocketCommandContext context, IGuildUser user, string reason)
        {
            //Get Muted role if it exists or create it if it doesn't exist
            var muteRole = Helper.DoesRoleExist(context.Guild, "Muted") ??
                           await context.Guild.CreateRoleAsync("Muted", GuildPermissions.None, Color.DarkGrey, false,
                               null);

            if (user.RoleIds.Any(role => role == muteRole.Id))
                return CustomFormats.CreateErrorEmbed($"{user} is already muted!");

            //Add muted role to the user
            await user.AddRoleAsync(muteRole);

            //Loop through every channel in the guild and deny the user from sending messages
            foreach (var channel in context.Guild.TextChannels)
                await channel.AddPermissionOverwriteAsync(user, new OverwritePermissions(sendMessages: PermValue.Deny));

            var caseId = await GenerateModCaseId(context.Guild.Id);

            //Create modCase
            var modCase = new ModCase(context, user, caseId, PunishmentType.Mute, reason);
            await _botContext.ModCases.AddAsync(modCase);
            await _botContext.SaveChangesAsync();
            await SendModLog(context.Guild, modCase);

            return ModerationFormats.CreateModerationEmbed(user, $"{user} muted",
                $"{user} has been muted for: {reason ?? "_No reason_"}.", Color.DarkGrey);
        }


        /// <summary>
        ///     Unmutes specified user.
        /// </summary>
        public static async Task<Embed> UnmuteAsync(SocketCommandContext context, IGuildUser user)
        {
            //Get Muted role if it exists
            var muteRole = Helper.DoesRoleExist(user.Guild, "Muted");
            if (muteRole == null)
                return CustomFormats.CreateErrorEmbed("Your server doesn't have a **'Muted'** role!");

            if (user.RoleIds.All(role => role != muteRole.Id))
                return CustomFormats.CreateErrorEmbed($"{user} is not muted!");

            //Loop through every channel in the guild and remove the permission overwrite
            foreach (var channel in context.Guild.TextChannels) await channel.RemovePermissionOverwriteAsync(user);

            //Remove muted role from user
            await user.RemoveRoleAsync(muteRole);
            return ModerationFormats.CreateModerationEmbed(user, $"{user} unmuted", $"{user} has been unmuted.",
                0x268618);
        }


        /// <summary>
        ///     Voice mutes specified user.
        ///     <para>Prevents the user from talking on voice channels.</para>
        /// </summary>
        public async Task<Embed> VoiceMuteAsync(SocketCommandContext context, IGuildUser user, string reason)
        {
            //If user is already muted, tell the command issuer that the specified user is already muted
            if (user.IsMuted) return CustomFormats.CreateErrorEmbed($"{user} is already voice muted.");

            //If user isn't already muted, then mute him
            await user.ModifyAsync(x => x.Mute = true, new RequestOptions { AuditLogReason = reason });

            var caseId = await GenerateModCaseId(context.Guild.Id);

            //Create modCase
            var modCase = new ModCase(context, user, caseId, PunishmentType.VMute, reason);
            await _botContext.ModCases.AddAsync(modCase);
            await _botContext.SaveChangesAsync();
            await SendModLog(context.Guild, modCase);
            return ModerationFormats.CreateModerationEmbed(user, $"{user} voice muted",
                $"{user} has been voice muted for: {reason ?? "_No reason_"}.",
                Color.DarkGrey);
        }


        /// <summary>
        ///     Voice unmutes specified user.
        /// </summary>
        public static async Task<Embed> UnmuteVoiceAsync(IGuildUser user)
        {
            //If user isn't muted, tell the command issuer that the specified user isn't muted
            if (!user.IsMuted) return CustomFormats.CreateErrorEmbed($"{user} isn't voice muted.");

            //If user is muted, then unmute him
            await user.ModifyAsync(x => x.Mute = false);
            return ModerationFormats.CreateModerationEmbed(user, $"{user} voice unmuted",
                $"{user} has been voice unmuted.",
                Color.DarkGrey);
        }

        #endregion


        #region Clean messages and slowmode

        /// <summary>
        ///     Removes X(count) messages from chat.
        /// </summary>
        public async Task CleanMessagesAsync(int count, SocketCommandContext context)
        {
            //We only delete 100 messages at a time to prevent bot from getting overloaded
            if (count > 100)
            {
                await context.Channel.SendMessageAsync(
                    embed: CustomFormats.CreateErrorEmbed(context.User.Mention +
                                                          " You cannot delete more than 100 messages at once"));
                return;
            }

            /* Saves all messages user specified in a variable, next
                   those messages are deleted and a message is sent to the textChannel
                   saying that X messages were deleted <- this message is deleted 2.3s later */

            //Save messages to delete in a variable
            var messages = await context.Channel.GetMessagesAsync(count + 1).FlattenAsync();
            var messagesToDelete = messages.ToList();

            if (messagesToDelete.Any(msg => DateTimeOffset.UtcNow - msg.CreatedAt.UtcDateTime > TimeSpan.FromDays(14)))
            {
                await context.Channel.SendMessageAsync(
                    embed: CustomFormats.CreateErrorEmbed(context.User.Mention +
                                                          " You cannot delete messages older than 2 weeks."));
                return;
            }

            //Delete messages to delete
            await context.Guild.GetTextChannel(context.Channel.Id).DeleteMessagesAsync(messagesToDelete,
                new RequestOptions { AuditLogReason = $"{context.User} requested message cleaning" });

            //Send success message that will disappear after 2300 milliseconds
            _interactivityService.DelayedSendMessageAndDeleteAsync(context.Channel, null,
                TimeSpan.FromMilliseconds(2300), null, false,
                CustomFormats.CreateBasicEmbed("Messages deleted",
                    $":white_check_mark: Deleted **{count}** messages.", 0x268618));
        }


        /// <summary>
        ///     Changes slowmode for specified text channel.
        /// </summary>
        public static async Task SlowmodeAsync(ITextChannel channel, int interval, SocketCommandContext context)
        {
            await ((SocketTextChannel)channel).ModifyAsync(x => x.SlowModeInterval = interval);
            await context.Channel.SendMessageAsync(
                embed: CustomFormats.CreateBasicEmbed("Slowmode changed", "", 0x268618));
        }

        #endregion


        #region Send mod logs, punishment DM and generate mod case ID

        public async Task SendModLog(IGuild guild, ModCase modCase)
        {
            //Retrieve guild settings
            var guildSettings = _botContext.Guilds.AsNoTracking().FirstOrDefault(x => x.GuildId == guild.Id);

            if (guildSettings == null)
                return;

            if (guildSettings.ModerationChannel == 0)
                return;

            var moderationChannel = await guild.GetTextChannelAsync(guildSettings.ModerationChannel);
            await moderationChannel.SendMessageAsync(embed: ModerationFormats.ModLogEmbed(modCase));
        }


        public static async Task SendModLog(SocketTextChannel moderationChannel, ModCase modCase)
        {
            await moderationChannel.SendMessageAsync(embed: ModerationFormats.ModLogEmbed(modCase));
        }


        public static async Task SendPunishmentDm(IUser user, Embed embed)
        {
            try
            {
                //Inform the user that he was banned
                await user.SendMessageAsync(embed: embed);
            }
            catch (Exception)
            {
                //If the user doesn't have DM's enabled, catch the error
            }
        }


        //Generate mod case id
        public async Task<ulong> GenerateModCaseId(ulong guildId)
        {
            var lastEntry = await _botContext.ModCases.AsNoTracking().AsAsyncEnumerable()
                .LastOrDefaultAsync(x => x.GuildId == guildId);
            return lastEntry.ModCaseId + 1;
        }

        #endregion
    }
}