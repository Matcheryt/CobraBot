using System;
using System.Linq;
using System.Threading.Tasks;
using CobraBot.Handlers;
using CobraBot.Helpers;
using Discord;
using Discord.Commands;
using Discord.Net;
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
            var channelToMessage = DatabaseHandler.GetChannel(user.Guild.Id);
            
            if (channelToMessage == null)
                return;

            await user.Guild.GetTextChannel(Convert.ToUInt64(channelToMessage)).SendMessageAsync(embed: await Helper.CreateBasicEmbed("User joined", $"{user.Username} has joined the server!", Color.Green));
        }

        /// <summary>Fired whenever someone leaves the server.
        /// <para>Used to log a message to a specific text channel.</para>
        /// </summary>
        public async Task UserLeftServer(SocketGuildUser user)
        {
            var channelToMessage = DatabaseHandler.GetChannel(user.Guild.Id);

            if (channelToMessage == null)
                return;

            await user.Guild.GetTextChannel(Convert.ToUInt64(channelToMessage)).SendMessageAsync(embed: await Helper.CreateBasicEmbed("User left", $"{user.Username} has left the server!", Color.DarkGrey));
        }

        /// <summary>Ban specified user from the server with reason.
        /// </summary>
        public async Task<Embed> BanAsync(IUser user, int pruneDays, [Remainder] string reason, SocketCommandContext context)
        {
            await context.Message.DeleteAsync();

            if (pruneDays < 0 || pruneDays > 7)
                return await Helper.CreateErrorEmbed("Prune days must be between 0 and 7");

            var isBanned = await context.Guild.GetBanAsync(user);
            if (isBanned != null)
                return await Helper.CreateErrorEmbed($"{user.Username} is already banned!");

            await context.Guild.AddBanAsync(user, pruneDays, reason);
            await user.SendMessageAsync($"You were banned from '{context.Guild.Name}' for: {reason}");
            return await Helper.CreateBasicEmbed($"{user.Username} banned", $"{user.Username} was banned successfully for: {reason}", Color.DarkGreen);
        }

        /* -------- WORK IN PROGRESS --------
        /// <summary>Unbans specified user from the server.
        /// </summary>
        public async Task<Embed> UnbanAsync(IUser bannedUser, SocketCommandContext context)
        {
            await context.Message.DeleteAsync();

            var isBanned = await GetBanSafeAsync(context.Guild, bannedUser);
            if (isBanned == null)
                return await Helper.CreateErrorEmbed($"{bannedUser.Username} is not banned!");

            await context.Guild.RemoveBanAsync(bannedUser);
            return await Helper.CreateBasicEmbed($"{bannedUser.Username} unbanned", $"{bannedUser.Username} was banned successfully", Color.DarkGreen);
        }
          ------------------------------------*/

        /// <summary>Kick specified user from the server with reason.
        /// </summary>
        public async Task<Embed> KickAsync(IGuildUser user, [Remainder]string reason, SocketCommandContext context)
        {
            await context.Message.DeleteAsync();

            await user.KickAsync(reason);
            await user.SendMessageAsync($"You were kicked from '{context.Guild.Name}' for: {reason}");
            return await Helper.CreateBasicEmbed($"{user.Username} kicked", $"{user.Username} was kicked from the server for: {reason}.", Color.DarkGreen);
        }


        /// <summary>Removes X(count) messages from chat.
        /// </summary>
        public async Task<Embed> CleanMessagesAsync(int count, SocketCommandContext context)
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
                return await Helper.CreateBasicEmbed("Messaged deleted", "Deleted " + "**" + count + "**" + " messages :white_check_mark:", Color.DarkGreen);
            }
            else
            {
                return await Helper.CreateErrorEmbed(context.User.Mention + " You cannot delete more than 100 messages at once");
            }
        }

        /// <summary>Used to check if role exists.
        /// <para>Returns true if it exists, false if it doesn't.</para>
        /// </summary>
        bool DoesRoleExist(IGuild guild, [Remainder]string roleName)
        {
            var roles = guild.Roles;

            foreach (IRole role in roles)
            {
                if (role.Name.Contains (roleName))
                    return true;
            }

            return false;
        }

        /// <summary>Gives/removes role from specified user.
        /// </summary>
        public async Task<Embed> UpdateRoleAsync(IGuildUser user, char operation, [Remainder]string roleName)
        {
            if (!DoesRoleExist(user.Guild, roleName))
                return await Helper.CreateErrorEmbed($"Role {roleName} doesn't exist!");

            if (operation == '+')
            {
                await user.AddRoleAsync(user.Guild.Roles.FirstOrDefault(x => x.Name.Contains(roleName)));
                return await Helper.CreateBasicEmbed("Role added", $"Role {roleName} was successfully added to {user.Username}", Color.DarkGreen);
            }
            else if (operation == '-')
            {
                await user.RemoveRoleAsync(user.Guild.Roles.FirstOrDefault(x => x.Name.Contains(roleName)));
                return await Helper.CreateBasicEmbed("Role removed", $"Role {roleName} was successfully removed from {user.Username}", Color.DarkGreen);
            }
            else
            {
                return await Helper.CreateErrorEmbed("Invalid operation! Available operations are **+** (add) and **-** (remove).");
            }

        }

        /// <summary>Changes guild's bot prefix.
        /// </summary>
        public async Task<Embed> ChangePrefixAsync(string prefix, SocketCommandContext context)
        {
            //If user input == default
            if (prefix == "default")
            {
                //Check if the guild has custom prefix
                string currentPrefix = DatabaseHandler.GetPrefix(context.Guild.Id);

                //If the guild doesn't have custom prefix, return
                if (currentPrefix == null)
                {
                    return await Helper.CreateErrorEmbed("Bot prefix is already the default one!");
                }

                //If they have a custom prefix, remove it from database and consequently setting it to default
                DatabaseHandler.RemovePrefixFromDB(context.Guild.Id);
                return await Helper.CreateBasicEmbed("", "Bot prefix was reset to:  **-**", Color.DarkGreen);
            }

            //If user input is longer than 5, return
            if (prefix.Length > 5)
            {
                return await Helper.CreateErrorEmbed("Bot prefix can't be longer than 5 characters!");
            }

            //If every check passes, we add the new custom prefix to the database
            DatabaseHandler.AddPrefixToDB(context.Guild.Id, prefix);
            return await Helper.CreateBasicEmbed("Prefix Changed", $"Bot's prefix is now:  **{prefix}**", Color.DarkGreen);
        }

        public async Task<Embed> SetWelcomeChannel(ITextChannel textChannel)
        {                   
            DatabaseHandler.AddChannelToDB(textChannel.Guild.Id, textChannel.Id.ToString());
            return await Helper.CreateBasicEmbed("Welcome channel changed", $"Welcome channel is now {textChannel.Mention}", Color.DarkGreen);
        }
    }
}
