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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CobraBot.TypeReaders;

namespace CobraBot.Services
{
    /* Awful code ahead, I need to organize this better. It does the job in the meantime. */
    public sealed class SetupService
    {
        private readonly InteractivityService _interactivityService;
        private readonly BotContext _botContext;
        private readonly IServiceProvider _serviceProvider;

        public SetupService(InteractivityService interactivityService, BotContext botContext, DiscordSocketClient client, IServiceProvider serviceProvider)
        {
            _interactivityService = interactivityService;
            _botContext = botContext;
            _serviceProvider = serviceProvider;

            //Handle event when bot joins guild
            client.JoinedGuild += Client_JoinedGuild;
        }

        //Fired every time the bot joins a new guild
        private static async Task Client_JoinedGuild(SocketGuild guild)
        {
            if (guild.Owner == null)
                return;

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
                .WithSelectables(new Dictionary<IEmote, string>()
                {
                    [new Emoji("1️⃣")] = "Welcome Channel",
                    [new Emoji("2️⃣")] = "Moderation Channel",
                    [new Emoji("3️⃣")] = "Role on Join",
                    [new Emoji("4️⃣")] = "Custom Prefix",
                    [new Emoji("5️⃣")] = "Private Chats"
                })
                .WithUsers(context.User)
                .WithAllowCancel(true)
                .WithDeletion(DeletionOptions.AfterCapturedContext | DeletionOptions.Invalids);

            //Send the selection builder and save the result
            var result = await _interactivityService.SendSelectionAsync(selection.Build(), context.Channel, TimeSpan.FromMinutes(3));

            //We switch through every result
            switch (result.Value)
            {
                //If user wants to setup Welcome Channel
                case "Welcome Channel":
                    await WelcomeChannelSetup(context);
                    break;

                //If user wants to setup Moderation Channel
                case "Moderation Channel":
                    await ModerationSetup(context);
                    break;

                //If user wants to setup Role on Join
                case "Role on Join":
                    await RoleOnJoinSetup(context);
                    break;

                //If user wants to setup Custom Prefix
                case "Custom Prefix":
                    await PrefixSetup(context);
                    break;

                //If user wants to setup Private Chats
                case "Private Chats":
                    await PrivateChatSetup(context);
                    break;
            }
        }

        #region Menu options setup
        private async Task WelcomeChannelSetup(SocketCommandContext context)
        {
            var tmpMessage = await context.Channel.SendMessageAsync(embed: CustomFormats.CreateBasicEmbed(
                "Welcome Channel Setup",
                "Cobra will send messages to this channel when someone joins/leaves the server.\n" +
                "Please mention the #textChannel you want to setup as the Welcome Channel.\n" +
                "Type `reset` to reset the Welcome Channel thus disabling this functionality.",
                Color.Blue));

            var nextMessageResult = await _interactivityService.NextMessageAsync(x => x.Author == context.User);

            if (nextMessageResult.IsSuccess)
            {
                var msgContent = nextMessageResult.Value.Content;

                if (msgContent == "reset")
                {
                    await ChangeWelcomeChannel(context);
                    await context.Channel.SendMessageAsync(embed: CustomFormats.CreateBasicEmbed(
                        "Welcome channel changed",
                        "Welcome channel was reset.\nYour server doesn't have a welcome channel setup right now.",
                        Color.DarkMagenta));
                }
                else
                {
                    if (nextMessageResult.Value.MentionedChannels.Any())
                    {
                        if (nextMessageResult.Value.MentionedChannels.First() is ITextChannel textChannel)
                        {
                            await ChangeWelcomeChannel(context, textChannel);
                            await context.Channel.SendMessageAsync(embed: CustomFormats.CreateBasicEmbed("Welcome channel changed",
                                $"Welcome channel is now {textChannel.Mention}", 0x268618));
                        }
                        else
                        {
                            await context.Channel.SendMessageAsync(
                                embed: CustomFormats.CreateErrorEmbed("Invalid text channel!"));
                        }
                    }
                    else
                    {
                        await context.Channel.SendMessageAsync(
                            embed: CustomFormats.CreateErrorEmbed("No text channels mentioned!"));
                    }
                }

                await nextMessageResult.Value.DeleteAsync();
                await tmpMessage.DeleteAsync();
            }
        }


        private async Task RoleOnJoinSetup(SocketCommandContext context)
        {
            var tmpMessage = await context.Channel.SendMessageAsync(embed: CustomFormats.CreateBasicEmbed(
                "Role on Join Setup",
                "Cobra will automatically give the specified role when someone joins the server.\n" +
                "Please type the name or ID of the role you want to setup as the role on join.\n" +
                "Type `reset` to reset the role on join thus disabling this functionality.", Color.Blue));

            var nextMessageResult = await _interactivityService.NextMessageAsync(x => x.Author == context.User);

            if (nextMessageResult.IsSuccess)
            {
                var msgContent = nextMessageResult.Value.Content;

                if (msgContent == "reset")
                {
                    await ChangeRoleOnJoin(context);
                    await context.Channel.SendMessageAsync(embed: CustomFormats.CreateBasicEmbed("Role on join changed",
                        "Role on join was reset.\nYour server doesn't have a role on join setup right now.",
                        Color.DarkMagenta));
                }
                else
                {
                    var tr = new ExtendedRoleTypeReader();
                    var readResult = await tr.ReadAsync(context, msgContent, _serviceProvider);
                    
                    if (!readResult.IsSuccess)
                        await context.Channel.SendMessageAsync(embed: CustomFormats.CreateErrorEmbed("Unable to find role!"));
                    else
                    {
                        if (readResult.Values.First().Value is IRole role)
                        {
                            await ChangeRoleOnJoin(context, role);
                            await context.Channel.SendMessageAsync(
                                embed: CustomFormats.CreateBasicEmbed("Role on join changed",
                                    $"Role on join was set to **{role.Name}**", 0x268618));
                        }
                        else
                        {
                            await context.Channel.SendMessageAsync(embed: CustomFormats.CreateErrorEmbed("Unable to find role!"));
                        }
                    }
                }

                await nextMessageResult.Value.DeleteAsync();
                await tmpMessage.DeleteAsync();
            }
        }


        private async Task PrefixSetup(SocketCommandContext context)
        {
            var tmpMessage = await context.Channel.SendMessageAsync(embed: CustomFormats.CreateBasicEmbed(
                "Prefix Setup",
                "Please type the new prefix for your guild (type `default` to reset it)", Color.Blue));

            var nextMessageResult = await _interactivityService.NextMessageAsync(x => x.Author == context.User);

            if (nextMessageResult.IsSuccess)
            {
                await nextMessageResult.Value.DeleteAsync();
                await tmpMessage.DeleteAsync();
                await context.Channel.SendMessageAsync(embed: await ChangePrefix(context, nextMessageResult.Value.Content));
            }
        }


        private async Task ModerationSetup(SocketCommandContext context)
        {
            var tmpMessage = await context.Channel.SendMessageAsync(embed: CustomFormats.CreateBasicEmbed(
                "Moderation Channel Setup",
                "Cobra will send mod cases to the specified channel.\n" +
                "Please mention the #textChannel you want to setup as the Moderation Channel\n" +
                "Type `reset` to reset the Moderation Channel thus disabling this functionality.", Color.Blue));

            var nextMessageResult = await _interactivityService.NextMessageAsync(x => x.Author == context.User);

            if (nextMessageResult.IsSuccess)
            {
                var msgContent = nextMessageResult.Value.Content;

                if (msgContent == "reset")
                {
                    await ChangeModerationChannel(context);
                    await context.Channel.SendMessageAsync(embed: CustomFormats.CreateBasicEmbed(
                        "Moderation Channel changed",
                        "Moderation channel was reset.\nYour server doesn't have a moderation channel setup right now.",
                        Color.DarkMagenta));
                }
                else
                {
                    if (nextMessageResult.Value.MentionedChannels.Any())
                    {
                        if (nextMessageResult.Value.MentionedChannels.First() is ITextChannel textChannel)
                        {
                            await ChangeModerationChannel(context, textChannel);
                            await context.Channel.SendMessageAsync(embed: CustomFormats.CreateBasicEmbed("Moderation Channel changed",
                                $"Moderation channel is now {textChannel.Mention}", 0x268618));
                        }
                        else
                        {
                            await context.Channel.SendMessageAsync(
                                embed: CustomFormats.CreateErrorEmbed("Invalid text channel!"));
                        }
                    }
                    else
                    {
                        await context.Channel.SendMessageAsync(
                            embed: CustomFormats.CreateErrorEmbed("No text channels mentioned!"));
                    }
                }

                await nextMessageResult.Value.DeleteAsync();
                await tmpMessage.DeleteAsync();
            }
        }


        private async Task PrivateChatSetup(SocketCommandContext context)
        {
            var tmpMessage = await context.Channel.SendMessageAsync(embed: CustomFormats.CreateBasicEmbed(
                "Private Chats Setup",
                "Paste the ID of the category where you want private chats to be created.\n\n" +
                "If you don't have that category yet, create one category with the name you like and move it to where you would like the private chats to be created." +
                "Don't change any channel permissions as Cobra takes care of that\n\n" +
                "Type `reset` to reset the Private Chat category thus disabling this functionality.", Color.Blue));

            var nextMessageResult = await _interactivityService.NextMessageAsync(x => x.Author == context.User);

            if (nextMessageResult.IsSuccess)
            {
                var msgContent = nextMessageResult.Value.Content;

                if (msgContent == "reset")
                {
                    await ChangePrivateChat(context);
                    await context.Channel.SendMessageAsync(embed: CustomFormats.CreateBasicEmbed(
                        "Private Chat changed",
                        "Private Chat category was reset.\nYour server doesn't have private chats setup right now.",
                        Color.DarkMagenta));
                }
                else
                {
                    if (ulong.TryParse(msgContent, out var categoryId))
                    {
                        var category = context.Guild.GetCategoryChannel(categoryId);

                        await ChangePrivateChat(context, category);
                        await context.Channel.SendMessageAsync(embed: CustomFormats.CreateBasicEmbed("Private Chat changed",
                            $"Private chats will now appear under the category {category.Name}", 0x268618));
                    }
                    else
                    {
                        await context.Channel.SendMessageAsync(embed: CustomFormats.CreateErrorEmbed("Invalid category id!"));
                    }
                }

                await nextMessageResult.Value.DeleteAsync();
                await tmpMessage.DeleteAsync();
            }
        }
        #endregion


        /// <summary> Updates guilds' prefix. </summary>
        /// <param name="context"> The command context. </param>
        /// <param name="prefix"> The new prefix. </param>
        public async Task<Embed> ChangePrefix(SocketCommandContext context, string prefix)
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


        /// <summary> Updates private chat category in database. If no category is specified, the private chat category will be reset. </summary>
        /// <param name="context"> The command context. </param>
        /// <param name="category"> The category to set as the private chat category. </param>
        public async Task ChangePrivateChat(SocketCommandContext context, SocketCategoryChannel category = null)
        {
            var guildSettings = await _botContext.GetGuildSettings(context.Guild.Id);

            if (category?.Id == guildSettings.PrivChannelsCategory)
            {
                await context.Channel.SendMessageAsync(
                    embed: CustomFormats.CreateErrorEmbed($"Private chats category is already {category.Name}!"));
                return;
            }

            if (category == null)
                guildSettings.PrivChannelsCategory = 0;
            else
                guildSettings.PrivChannelsCategory = category.Id;

            await _botContext.SaveChangesAsync();
        }


        /// <summary> Updates welcome channel in database. If no ITextChannel is specified, the welcome channel for the specified guild will be reset. </summary>
        /// <param name="context"> The command context. </param>
        /// <param name="textChannel"> The ITextChannel to set as the welcome channel. </param>
        public async Task ChangeWelcomeChannel(SocketCommandContext context, ITextChannel textChannel = null)
        {
            var guildSettings = await _botContext.GetGuildSettings(context.Guild.Id);

            if (textChannel?.Id == guildSettings.WelcomeChannel)
            {
                await context.Channel.SendMessageAsync(
                    embed: CustomFormats.CreateErrorEmbed($"Welcome channel is already {textChannel.Name}!"));
                return;
            }

            if (textChannel == null)
                guildSettings.WelcomeChannel = 0;
            else
                guildSettings.WelcomeChannel = textChannel.Id;

            await _botContext.SaveChangesAsync();
        }


        /// <summary> Updates moderation channel in database. If no ITextChannel is specified, the moderation channel for the specified guild will be reset. </summary>
        /// <param name="context"> The command context. </param>
        /// <param name="textChannel"> The ITextChannel to set as the moderation channel. </param>
        public async Task ChangeModerationChannel(SocketCommandContext context, ITextChannel textChannel = null)
        {
            var guildSettings = await _botContext.GetGuildSettings(context.Guild.Id);

            if (textChannel?.Id == guildSettings.ModerationChannel)
            {
                await context.Channel.SendMessageAsync(
                    embed: CustomFormats.CreateErrorEmbed($"Moderation channel is already {textChannel.Name}!"));
                return;
            }

            if (textChannel == null)
                guildSettings.ModerationChannel = 0;
            else
                guildSettings.ModerationChannel = textChannel.Id;

            await _botContext.SaveChangesAsync();
        }


        /// <summary> Updates role on join in database. If no IRole is specified, the role on join for the specified guild will be reset. </summary>
        /// <param name="context"> The command context. </param>
        /// <param name="role"> The IRole to set as the role on join. </param>
        public async Task ChangeRoleOnJoin(SocketCommandContext context, IRole role = null)
        {
            var guildSettings = await _botContext.GetGuildSettings(context.Guild.Id);

            if (role?.Id == guildSettings.RoleOnJoin)
            {
                await context.Channel.SendMessageAsync(
                    embed: CustomFormats.CreateErrorEmbed($"Role on join is already {role.Name}!"));
                return;
            }

            if (role == null)
                guildSettings.RoleOnJoin = 0;
            else
                guildSettings.RoleOnJoin = role.Id;

            await _botContext.SaveChangesAsync();
        }
    }
}