using System;
using System.Linq;
using System.Threading.Tasks;
using CobraBot.Common;
using CobraBot.Database;
using CobraBot.Helpers;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Interactivity;
using Microsoft.EntityFrameworkCore;
using Z.EntityFramework.Plus;

namespace CobraBot.Services
{
    public sealed class ModerationService
    {
        private readonly BotContext _botContext;
        private readonly InteractivityService _interactivityService;
        
        public ModerationService(BotContext botContext, InteractivityService interactivityService, DiscordSocketClient client)
        {
            _botContext = botContext;
            _interactivityService = interactivityService;

            //Events
            client.UserJoined += UserJoinedServer;
            client.UserLeft += UserLeftServer;
        }
        
        /// <summary>Fired whenever someone joins the server.
        /// <para>Used to log a message to a specific text channel.</para>
        /// </summary>
        public async Task UserJoinedServer(SocketGuildUser user)
        {
            //Retrieve guild settings
            var guildSettings = _botContext.Guilds.AsNoTracking().Where(x => x.GuildId == user.Guild.Id).FromCache(user.Guild.Id.ToString()).FirstOrDefault();

            if (guildSettings is null)
                return;
            
            //Check if there is a valid role and give that role to the user
            if (guildSettings.RoleOnJoin != null && (Helper.DoesRoleExist(user.Guild, guildSettings.RoleOnJoin) != null))
                await user.AddRoleAsync(user.Guild.Roles.SingleOrDefault(x => x.Name.Contains(guildSettings.RoleOnJoin)));

            //Announce to WelcomeChannel that the user joined the server
            if (guildSettings.WelcomeChannel != 0)
                await user.Guild.GetTextChannel(Convert.ToUInt64(guildSettings.WelcomeChannel)).SendMessageAsync(embed: EmbedFormats.CreateBasicEmbed("User joined", $"{user} has joined the server!", Color.Green));
        }

        /// <summary>Fired whenever someone leaves the server.
        /// <para>Used to log a message to a specific text channel.</para>
        /// </summary>
        public async Task UserLeftServer(SocketGuildUser user)
        {
            //Retrieve guild settings
            var guildSettings = _botContext.Guilds.AsNoTracking().Where(x => x.GuildId == user.Guild.Id).FromCache(user.Guild.Id.ToString()).FirstOrDefault();

            if (guildSettings is null)
                return;

            //If we do have a valid channel, announce that the user left the server
            if (guildSettings.WelcomeChannel != 0)
                await user.Guild.GetTextChannel(Convert.ToUInt64(guildSettings.WelcomeChannel)).SendMessageAsync(embed: EmbedFormats.CreateBasicEmbed("User left", $"{user} has left the server!", Color.DarkGrey));
        }

        /// <summary>Ban specified user from the server with reason.
        /// </summary>
        public static async Task<Embed> BanAsync(IUser user, int pruneDays, string reason, SocketCommandContext context)
        {
            if (((IGuildUser)user).GuildPermissions.Administrator)
                return EmbedFormats.CreateErrorEmbed("The user you're trying to ban is a mod/admin.");

            if (!Helper.BotHasHigherHierarchy((SocketGuildUser)user, context))
                return EmbedFormats.CreateErrorEmbed("Cobra's role isn't high enough to moderate specified user. Move 'Cobra' role up above other roles.");

            if (pruneDays < 0 || pruneDays > 7)
                return EmbedFormats.CreateErrorEmbed("Prune days must be between 0 and 7");

            //Check if user is already banned
            var isBanned = await context.Guild.GetBanAsync(user);
            if (isBanned != null)
                return EmbedFormats.CreateErrorEmbed($"{user.Username} is already banned!");

            //Ban user
            await context.Guild.AddBanAsync(user, pruneDays, reason);
            return EmbedFormats.CreateModerationEmbed(user,$"{user} kicked", $"{user} was kicked from the server for: {reason}.", Color.DarkGrey);
        }

        /* -------- WORK IN PROGRESS --------
        /// <summary>Unbans specified user from the server.
        /// </summary>
        public async Task<Embed> UnbanAsync(IUser bannedUser, SocketCommandContext context)
        {
            await context.Message.DeleteAsync();

            var isBanned = await GetBanSafeAsync(context.Guild, bannedUser);
            if (isBanned == null)
                return EmbedFormats.ErrorEmbed($"{bannedUser.Username} is not banned!");

            await context.Guild.RemoveBanAsync(bannedUser);
            return EmbedFormats.CreateBasicEmbed($"{bannedUser.Username} unbanned", $"{bannedUser.Username} was banned successfully", Color.DarkGreen);
        }
          ------------------------------------*/

        /// <summary>Kick specified user from the server with reason.
        /// </summary>
        public static async Task<Embed> KickAsync(IGuildUser user, string reason, SocketCommandContext context)
        {
            if (user.GuildPermissions.Administrator)
                return EmbedFormats.CreateErrorEmbed("The user you're trying to kick is a mod/admin.");

            if (!Helper.BotHasHigherHierarchy((SocketGuildUser)user, context))
                return EmbedFormats.CreateErrorEmbed("Cobra's role isn't high enough to moderate specified user. Move 'Cobra' role up above other roles.");

            //If all checks pass, kick user
            await user.KickAsync(reason);
            return EmbedFormats.CreateModerationEmbed(user,$"{user} kicked", $"{user} was kicked from the server for: {reason}.", Color.DarkGrey);
        }

        /// <summary>Mutes specified user.
        /// <para>Prevents the user from sending chat messages.</para>
        /// </summary>
        public static async Task<Embed> MuteAsync(IGuildUser user, SocketCommandContext context)
        {
            if (user.GuildPermissions.Administrator)
                return EmbedFormats.CreateErrorEmbed("The user you're trying to mute is a mod/admin.");

            if (!Helper.BotHasHigherHierarchy((SocketGuildUser)user, context))
                return EmbedFormats.CreateErrorEmbed("Cobra's role isn't high enough to moderate specified user. Move 'Cobra' role up above other roles.");

            //Get Muted role if it exists or create it if it doesn't exist
            var muteRole = Helper.DoesRoleExist(user.Guild, "Muted") ?? await user.Guild.CreateRoleAsync("Muted", GuildPermissions.None, Color.DarkGrey, false, null);

            //Add muted role to the user
            await user.AddRoleAsync(muteRole);
            return EmbedFormats.CreateModerationEmbed(user, $"{user} muted", $"{user} has been muted.", Color.DarkGrey);
        }

        /// <summary>Unmutes specified user.
        /// </summary>
        public static async Task<Embed> UnmuteAsync(IGuildUser user, SocketCommandContext context)
        {
            if (user.GuildPermissions.Administrator)
                return EmbedFormats.CreateErrorEmbed("The user you're trying to mute is a mod/admin.");

            if (!Helper.BotHasHigherHierarchy((SocketGuildUser)user, context))
                return EmbedFormats.CreateErrorEmbed("Cobra's role isn't high enough to moderate specified user. Move 'Cobra' role up above other roles.");

            //Get Muted role if it exists
            var muteRole = Helper.DoesRoleExist(user.Guild, "Muted");
            if (muteRole == null)
                return EmbedFormats.CreateErrorEmbed("Your server doesn't have a **'Muted'** role!");

            //Remove muted role from user
            await user.RemoveRoleAsync(muteRole);
            return EmbedFormats.CreateModerationEmbed(user, $"{user} unmuted", $"{user} has been unmuted.", Color.DarkGreen);
        }

        /// <summary>Voice mutes specified user.
        /// <para>Prevents the user from talking on voice channels.</para>
        /// </summary>
        public static async Task<Embed> VoiceMuteAsync(IGuildUser user, SocketCommandContext context)
        {
            if (user.GuildPermissions.Administrator)
                return EmbedFormats.CreateErrorEmbed("The user you're trying to mute is a mod/admin.");

            if (!Helper.BotHasHigherHierarchy((SocketGuildUser)user, context))
                return EmbedFormats.CreateErrorEmbed("Cobra's role isn't high enough to moderate specified user. Move 'Cobra' role up above other roles.");

            //If user is already muted, tell the command issuer that the specified user is already muted
            if (user.IsMuted) return EmbedFormats.CreateErrorEmbed($"{user} is already voice muted.");
            
            //If user isn't already muted, then mute him
            await user.ModifyAsync(x => x.Mute = true);
            return EmbedFormats.CreateModerationEmbed(user, $"{user} voice muted", $"{user} has been voice muted.",
                Color.DarkGrey);
        }

        /// <summary>Voice unmutes specified user.
        /// </summary>
        public static async Task<Embed> UnmuteVoiceAsync(IGuildUser user, SocketCommandContext context)
        {
            if (user.GuildPermissions.Administrator)
                return EmbedFormats.CreateErrorEmbed("The user you're trying to mute is a mod/admin.");

            if (!Helper.BotHasHigherHierarchy((SocketGuildUser)user, context))
                return EmbedFormats.CreateErrorEmbed("Cobra's role isn't high enough to moderate specified user. Move 'Cobra' role up above other roles.");

            //If user isn't muted, tell the command issuer that the specified user isn't muted
            if (!user.IsMuted) return EmbedFormats.CreateErrorEmbed($"{user} isn't voice muted.");

            //If user is muted, then unmute him
            await user.ModifyAsync(x => x.Mute = false);
            return EmbedFormats.CreateModerationEmbed(user, $"{user} voice unmuted", $"{user} has been voice unmuted.",
                Color.DarkGrey);
        }
        

        /// <summary>Removes X(count) messages from chat.
        /// </summary>
        public async Task CleanMessagesAsync(int count, SocketCommandContext context)
        {
            await context.Message.DeleteAsync();

            //We only delete 100 messages at a time to prevent bot from getting overloaded
            if (count <= 100)
            {
                /* Saves all messages user specified in a variable, next
                   those messages are deleted and a message is sent to the textChannel
                   saying that X messages were deleted <- this message is deleted 2.3s later */

                //Save messages to delete in a variable
                var messagesToDelete = await context.Channel.GetMessagesAsync(count+1).FlattenAsync();

                //Delete messages to delete
                await context.Guild.GetTextChannel(context.Channel.Id).DeleteMessagesAsync(messagesToDelete);

                //Send success message that will disappear after 2300 milliseconds
                _interactivityService.DelayedSendMessageAndDeleteAsync(context.Channel, null,
                    TimeSpan.FromMilliseconds(2300), null, false,
                    EmbedFormats.CreateBasicEmbed("Messages deleted",
                        $":white_check_mark: Deleted **{count}** messages.", Color.DarkGreen));
            }
            else
            {
                await context.Channel.SendMessageAsync(embed: EmbedFormats.CreateErrorEmbed(context.User.Mention + " You cannot delete more than 100 messages at once"));
            }
        }


        /// <summary>Gives/removes role from specified user.
        /// </summary>
        public static async Task<Embed> UpdateRoleAsync(IGuildUser user, char operation, string roleName)
        {
            //Get role which name equals roleName
            var roleToUpdate = Helper.DoesRoleExist(user.Guild, roleName);

            //If there isn't any role, return
            if (roleToUpdate == null)
                return EmbedFormats.CreateErrorEmbed($"Role {roleName} doesn't exist!");

            switch (operation)
            {
                case '+':
                    await user.AddRoleAsync(roleToUpdate);
                    return EmbedFormats.CreateBasicEmbed("Role added", $"Role {roleToUpdate.Name} was successfully added to {user.Username}", Color.DarkGreen);
                
                case '-':
                    await user.RemoveRoleAsync(roleToUpdate);
                    return EmbedFormats.CreateBasicEmbed("Role removed", $"Role {roleToUpdate.Name} was successfully removed from {user.Username}", Color.DarkGreen);
                
                default:
                    return EmbedFormats.CreateErrorEmbed("Invalid operation! Available operations are **+** (add) and **-** (remove).");
            }
        }

        /// <summary>Changes slowmode for specified text channel.
        /// </summary>
        public static async Task SlowmodeAsync(ITextChannel channel, int interval, SocketCommandContext context)
        {
            await ((SocketTextChannel) channel).ModifyAsync(x => x.SlowModeInterval = interval);
            await context.Channel.SendMessageAsync(
                embed: EmbedFormats.CreateBasicEmbed("Slowmode changed", "", Color.DarkGreen));
        }
    }
}