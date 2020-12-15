using System;
using System.Linq;
using System.Threading.Tasks;
using CobraBot.Common;
using CobraBot.Handlers;
using CobraBot.Helpers;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace CobraBot.Services
{
    public sealed class ModerationService
    {
        /// <summary>Fired whenever someone joins the server.
        /// <para>Used to log a message to a specific text channel.</para>
        /// </summary>
        public async Task UserJoinedServer(SocketGuildUser user)
        {
            //Retrieve guild settings
            var guildSettings = DatabaseHandler.RetrieveGuildSettings(user.Guild.Id);

            //Check if there is a valid role and give that role to the user
            if (guildSettings.RoleOnJoin != null && (Helper.DoesRoleExist(user.Guild, guildSettings.RoleOnJoin) != null))
                await user.AddRoleAsync(user.Guild.Roles.SingleOrDefault(x => x.Name.Contains(guildSettings.RoleOnJoin)));

            //Announce to JoinLeaveChannel that the user joined the server
            if (guildSettings.JoinLeaveChannel != null)
                await user.Guild.GetTextChannel(Convert.ToUInt64(guildSettings.JoinLeaveChannel)).SendMessageAsync(embed: EmbedFormats.CreateBasicEmbed("User joined", $"{user} has joined the server!", Color.Green));
        }

        /// <summary>Fired whenever someone leaves the server.
        /// <para>Used to log a message to a specific text channel.</para>
        /// </summary>
        public async Task UserLeftServer(SocketGuildUser user)
        {
            //Retrieve JoinLeaveChannel
            var channelToMessage = DatabaseHandler.RetrieveGuildSettings(user.Guild.Id).JoinLeaveChannel;

            //If we don't have a valid channel, return
            if (channelToMessage == null)
                return;

            //If we do have a valid channel, announce that the user left the server
            await user.Guild.GetTextChannel(Convert.ToUInt64(channelToMessage)).SendMessageAsync(embed: EmbedFormats.CreateBasicEmbed("User left", $"{user} has left the server!", Color.DarkGrey));
        }

        /// <summary>Ban specified user from the server with reason.
        /// </summary>
        public async Task<Embed> BanAsync(IUser user, int pruneDays, string reason, SocketCommandContext context)
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
                return EmbedFormats.CreateErrorEmbed($"{bannedUser.Username} is not banned!");

            await context.Guild.RemoveBanAsync(bannedUser);
            return EmbedFormats.CreateBasicEmbed($"{bannedUser.Username} unbanned", $"{bannedUser.Username} was banned successfully", Color.DarkGreen);
        }
          ------------------------------------*/

        /// <summary>Kick specified user from the server with reason.
        /// </summary>
        public async Task<Embed> KickAsync(IGuildUser user, string reason, SocketCommandContext context)
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
        public async Task<Embed> MuteAsync(IGuildUser user, SocketCommandContext context)
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
                var messagesToDelete = await context.Channel.GetMessagesAsync(count).FlattenAsync();

                //Delete messages to delete
                await context.Guild.GetTextChannel(context.Channel.Id).DeleteMessagesAsync(messagesToDelete);

                //Message sent informing that X messages were deleted
                var sentMessage = await context.Channel.SendMessageAsync(embed: EmbedFormats.CreateBasicEmbed(
                    "Messages deleted", "Deleted " + "**" + count + "**" + " messages :white_check_mark:",
                    Color.DarkGreen));
                await Task.Delay(2300);
                await sentMessage.DeleteAsync();
            }
            else
            {
                await context.Channel.SendMessageAsync(embed: EmbedFormats.CreateErrorEmbed(context.User.Mention + " You cannot delete more than 100 messages at once"));
            }
        }


        /// <summary>Gives/removes role from specified user.
        /// </summary>
        public async Task<Embed> UpdateRoleAsync(IGuildUser user, char operation, string roleName)
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

        /// <summary>Changes guild's bot prefix.
        /// </summary>
        public static Embed ChangePrefixAsync(string prefix, SocketCommandContext context)
        {
            //If user input == default
            if (prefix == "default")
            {
                //Check if the guild has custom prefix
                string currentPrefix = DatabaseHandler.RetrieveGuildSettings(context.Guild.Id).Prefix;

                //If the guild doesn't have custom prefix, return
                if (currentPrefix == null)
                {
                    return EmbedFormats.CreateErrorEmbed("Bot prefix is already the default one!");
                }

                //If they have a custom prefix, remove it from database and consequently setting it to default
                DatabaseHandler.UpdatePrefixDb(context.Guild.Id, '-');
                return EmbedFormats.CreateBasicEmbed("", "Bot prefix was reset to:  **-**", Color.DarkGreen);
            }

            //If user input is longer than 5, return
            if (prefix.Length > 5)
            {
                return EmbedFormats.CreateErrorEmbed("Bot prefix can't be longer than 5 characters!");
            }

            //If every check passes, we add the new custom prefix to the database
            DatabaseHandler.UpdatePrefixDb(context.Guild.Id, '+', prefix);
            return EmbedFormats.CreateBasicEmbed("Prefix Changed", $"Cobra's prefix is now:  **{prefix}**", Color.DarkGreen);
        }

        /// <summary>Sets guild's welcome channel.
        /// </summary>
        public static Embed SetWelcomeChannel(ITextChannel textChannel)
        {                   
            DatabaseHandler.UpdateChannelDb(textChannel.Guild.Id, '+', textChannel.Id.ToString());
            return EmbedFormats.CreateBasicEmbed("Welcome channel changed", $"Welcome channel is now {textChannel.Mention}", Color.DarkGreen);
        }

        /// <summary>Resets guild's welcome channel.
        /// </summary>
        public static Embed ResetWelcomeChannel(SocketCommandContext context)
        {
            DatabaseHandler.UpdateChannelDb(context.Guild.Id, '-');
            return EmbedFormats.CreateBasicEmbed("Welcome channel changed",
                "Welcome channel was reset.\nYour server doesn't have a welcome channel setup right now",
                Color.DarkGreen);
        }

        /// <summary>Changes role that users receive when they join the server.
        /// </summary>
        public static Embed SetRoleOnJoin(IGuild guild, string roleName)
        {
            var role = Helper.DoesRoleExist(guild, roleName);

            if (role == null)
                return EmbedFormats.CreateErrorEmbed($"Role **{roleName}** doesn't exist!");

            DatabaseHandler.UpdateRoleOnJoinDB(guild.Id, '+', role.Name);
            return EmbedFormats.CreateBasicEmbed("Role on join changed", $"Role on join was set to **{role.Name}**", Color.DarkGreen);
        }

        /// <summary>Changes role that users receive when they join the server.
        /// </summary>
        public static Embed ResetRoleOnJoin(SocketCommandContext context)
        {
            DatabaseHandler.UpdateRoleOnJoinDB(context.Guild.Id, '-');
            return EmbedFormats.CreateBasicEmbed("Role on join changed",
                "Role on join was reset\nYour server doesn't have a role on join setup right now", Color.DarkGreen);
        }
    }
}