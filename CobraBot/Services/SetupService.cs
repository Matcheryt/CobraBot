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

using CobraBot.Common.EmbedFormats;
using CobraBot.Database;
using CobraBot.Helpers;
using Discord;
using Discord.Commands;
using Discord.Net;
using Discord.WebSocket;
using Interactivity;
using Interactivity.Selection;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CobraBot.Services
{
    public sealed class SetupService
    {
        private readonly InteractivityService _interactivityService;
        private readonly BotContext _botContext;

        public SetupService(InteractivityService interactivityService, BotContext botContext, DiscordSocketClient client)
        {
            _interactivityService = interactivityService;
            _botContext = botContext;

            //Handle event when bot joins guild
            client.JoinedGuild += Client_JoinedGuild;
        }

        //Fired every time the bot joins a new guild
        private static async Task Client_JoinedGuild(SocketGuild guild)
        {
            try
            {
                //We send this message for the guild owner
                await guild.Owner.SendMessageAsync(embed: CustomFormats.CreateBasicEmbed("Hello, I'm Cobra! 👋",
                    "Thank you for adding me to your server!\nTo get started, type `-setup` in any text channel of your guild." +
                    "\nIf you need help, you can join the [support server](https://discord.gg/pbkdG7gYeu).",
                    0x268618));
            }
            catch (HttpException)
            {
                //If the user doesn't have DM's enabled, catch the error
            }
        }


        /// <summary>Starts setup process where guild admins can easily setup their specific guild settings.
        /// </summary>
        public async Task SetupAsync(SocketCommandContext context)
        {
            //Delete the message that invoked this command
            await context.Message.DeleteAsync();

            //Selection builder used to know which settings the admin wants to change
            var selection = new ReactionSelectionBuilder<string>()
                .WithTitle("Cobra Setup")
                .WithValues("Welcome Channel", "Role on Join", "Custom Prefix", "Moderation Channel")
                .WithEmotes(new Emoji("1️⃣"), new Emoji("2️⃣"), new Emoji("3️⃣"), new Emoji("4️⃣"))
                .WithUsers(context.User)
                .WithAllowCancel(true)
                .WithDeletion(DeletionOptions.AfterCapturedContext | DeletionOptions.Invalids);

            //Send the selection builder and save the result
            var result = await _interactivityService.SendSelectionAsync(selection.Build(), context.Channel, TimeSpan.FromMinutes(3));

            IMessage tmpMessage;
            InteractivityResult<SocketMessage> nextMessageResult;

            //We switch through every result
            switch (result.Value)
            {
                //If user wants to setup Welcome Channel
                case "Welcome Channel":

                    tmpMessage = await context.Channel.SendMessageAsync(embed: CustomFormats.CreateBasicEmbed(
                        "Welcome Channel Setup",
                        "Cobra will send messages to this channel when someone joins/leaves the server.\nPlease mention the #textChannel you want to setup as the Welcome Channel", Color.Blue));

                    nextMessageResult = await _interactivityService.NextMessageAsync(x => x.Author == context.User);

                    if (nextMessageResult.IsSuccess)
                    {
                        var textChannel = (ITextChannel)nextMessageResult.Value.MentionedChannels.FirstOrDefault();

                        if (textChannel == null)
                        {
                            await context.Channel.SendMessageAsync(
                                embed: CustomFormats.CreateErrorEmbed("**No channel specified!** Please try again."));
                            return;
                        }

                        await nextMessageResult.Value.DeleteAsync();
                        await tmpMessage.DeleteAsync();
                        await context.Channel.SendMessageAsync(embed: await SetWelcomeChannel(textChannel));
                    }
                    break;

                //If user wants to setup Role on Join
                case "Role on Join":

                    tmpMessage = await context.Channel.SendMessageAsync(embed: CustomFormats.CreateBasicEmbed(
                        "Role on Join Setup",
                        "Cobra will automatically give the specified role when someone joins the server.\nPlease type the name or ID of the role you want to setup as the role on join", Color.Blue));

                    nextMessageResult = await _interactivityService.NextMessageAsync(x => x.Author == context.User);

                    if (nextMessageResult.IsSuccess)
                    {
                        var role = Helper.DoesRoleExist(context.Guild, nextMessageResult.Value.Content);
                        if (role == null)
                        {
                            await context.Channel.SendMessageAsync(
                                embed: CustomFormats.CreateErrorEmbed("Unable to find role!"));
                            return;
                        }

                        await nextMessageResult.Value.DeleteAsync();
                        await tmpMessage.DeleteAsync();
                        await context.Channel.SendMessageAsync(embed: await SetRoleOnJoin(context.Guild, role));
                    }
                    break;

                //If user wants to setup Custom Prefix
                case "Custom Prefix":

                    tmpMessage = await context.Channel.SendMessageAsync(embed: CustomFormats.CreateBasicEmbed(
                        "Prefix Setup",
                        "Please type the new prefix for your guild (type `default` to reset it)", Color.Blue));

                    nextMessageResult = await _interactivityService.NextMessageAsync(x => x.Author == context.User);

                    if (nextMessageResult.IsSuccess)
                    {
                        await nextMessageResult.Value.DeleteAsync();
                        await tmpMessage.DeleteAsync();
                        await context.Channel.SendMessageAsync(embed: await ChangePrefixAsync(nextMessageResult.Value.Content, context));
                    }
                    break;

                //If user wants to setup Moderation Channel
                case "Moderation Channel":

                    tmpMessage = await context.Channel.SendMessageAsync(embed: CustomFormats.CreateBasicEmbed(
                        "Moderation Channel Setup",
                        "Cobra will send mod cases to the specified channel.\nPlease mention the #textChannel you want to setup as the Moderation Channel", Color.Blue));

                    nextMessageResult = await _interactivityService.NextMessageAsync(x => x.Author == context.User);

                    if (nextMessageResult.IsSuccess)
                    {
                        var textChannel = (ITextChannel)nextMessageResult.Value.MentionedChannels.FirstOrDefault();

                        if (textChannel == null)
                        {
                            await context.Channel.SendMessageAsync(
                                embed: CustomFormats.CreateErrorEmbed("**No channel specified!** Please try again."));
                            return;
                        }

                        var guildSettings = await _botContext.GetGuildSettings(context.Guild.Id);

                        guildSettings.ModerationChannel = textChannel.Id;
                        await _botContext.SaveChangesAsync();

                        await nextMessageResult.Value.DeleteAsync();
                        await tmpMessage.DeleteAsync();
                        await context.Channel.SendMessageAsync(embed: CustomFormats.CreateBasicEmbed(
                            "Moderation channel changed", $"Moderation channel is now {textChannel.Mention}",
                            0x268618));
                    }
                    break;
            }
        }


        /// <summary>Changes guild's bot prefix.
        /// </summary>
        public async Task<Embed> ChangePrefixAsync(string prefix, SocketCommandContext context)
        {
            var guildSettings = await _botContext.GetGuildSettings(context.Guild.Id);

            //If user input == default
            if (prefix == "default")
            {
                //Check if the guild has custom prefix
                string currentPrefix = guildSettings.CustomPrefix;

                //If the guild doesn't have custom prefix, return
                if (currentPrefix == null)
                    return CustomFormats.CreateErrorEmbed("Bot prefix is already the default one!");


                //If they have a custom prefix, set it to null
                guildSettings.CustomPrefix = null;
                await _botContext.SaveChangesAsync();
                return CustomFormats.CreateBasicEmbed("Custom prefix changed", "Bot prefix was reset to:  **-**", 0x268618);
            }

            //If user input is longer than 5, return
            if (prefix.Length > 5)
                return CustomFormats.CreateErrorEmbed("Bot prefix can't be longer than 5 characters!");

            //If every check passes, we add the new custom prefix to the database
            guildSettings.CustomPrefix = prefix;
            await _botContext.SaveChangesAsync();

            return CustomFormats.CreateBasicEmbed("Custom prefix changed", $"Cobra's prefix is now:  **{prefix}**", 0x268618);
        }


        #region Welcome
        /// <summary>Sets guild's welcome channel.
        /// </summary>
        public async Task<Embed> SetWelcomeChannel(ITextChannel textChannel)
        {
            var guildSettings = await _botContext.GetGuildSettings(textChannel.Guild.Id);

            guildSettings.WelcomeChannel = textChannel.Id;
            await _botContext.SaveChangesAsync();

            return CustomFormats.CreateBasicEmbed("Welcome channel changed", $"Welcome channel is now {textChannel.Mention}", 0x268618);
        }


        /// <summary>Resets guild's welcome channel.
        /// </summary>
        public async Task<Embed> ResetWelcomeChannel(SocketCommandContext context)
        {
            var guildSettings = await _botContext.GetGuildSettings(context.Guild.Id);

            guildSettings.WelcomeChannel = 0;
            await _botContext.SaveChangesAsync();

            return CustomFormats.CreateBasicEmbed("Welcome channel changed",
                "Welcome channel was reset.\nYour server doesn't have a welcome channel setup right now.",
                Color.DarkMagenta);
        }
        #endregion


        #region Role on join
        /// <summary>Changes role that users receive when they join the server.
        /// </summary>
        public async Task<Embed> SetRoleOnJoin(IGuild guild, IRole role)
        {
            var guildSettings = await _botContext.GetGuildSettings(guild.Id);

            guildSettings.RoleOnJoin = role.Name;
            await _botContext.SaveChangesAsync();

            return CustomFormats.CreateBasicEmbed("Role on join changed", $"Role on join was set to **{role.Name}**", 0x268618);
        }


        /// <summary>Changes role that users receive when they join the server.
        /// </summary>
        public async Task<Embed> ResetRoleOnJoin(SocketCommandContext context)
        {
            var guildSettings = await _botContext.GetGuildSettings(context.Guild.Id);

            guildSettings.RoleOnJoin = null;
            await _botContext.SaveChangesAsync();

            return CustomFormats.CreateBasicEmbed("Role on join changed",
                "Role on join was reset.\nYour server doesn't have a role on join setup right now.", Color.DarkMagenta);
        }
        #endregion
    }
}