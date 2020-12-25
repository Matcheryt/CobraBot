using System;
using System.Linq;
using System.Threading.Tasks;
using CobraBot.Common;
using CobraBot.Database;
using CobraBot.Helpers;
using Discord;
using Discord.Commands;
using Interactivity;
using Interactivity.Selection;

namespace CobraBot.Services
{
    public sealed class SetupService
    {
        private readonly InteractivityService _interactivityService;
        private readonly BotContext _botContext;

        public SetupService(InteractivityService interactivityService, BotContext botContext)
        {
            _interactivityService = interactivityService;
            _botContext = botContext;
        }

        /// <summary>Starts setup process where guild admins can easily setup their specific guild settings.
        /// </summary>
        public async Task SetupAsync(SocketCommandContext context)
        {
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

            //We switch through every result
            switch (result.Value)
            {
                //If user wants to setup Welcome Channel
                case "Welcome Channel":

                    await context.Channel.SendMessageAsync(
                        "Please mention the #textChannel you want to setup as the Welcome Channel:");
                    
                    var welcomeResult = await _interactivityService.NextMessageAsync(x => x.Author == context.User);

                    if (welcomeResult.IsSuccess)
                    {
                        var textChannel = (ITextChannel)welcomeResult.Value.MentionedChannels.FirstOrDefault();

                        if (textChannel == null)
                        {
                            await context.Channel.SendMessageAsync(
                                embed: EmbedFormats.CreateErrorEmbed("**No channel specified!** Please try again."));
                            return;
                        }

                        var guildSettings = await _botContext.GetGuildSettings(context.Guild.Id);

                        guildSettings.WelcomeChannel = textChannel.Id;
                        await _botContext.SaveChangesAndExpireAsync(context.Guild.Id.ToString());
                        await context.Channel.SendMessageAsync(embed: EmbedFormats.CreateBasicEmbed(
                            "Welcome channel changed", $"Welcome channel is now {textChannel.Mention}",
                            Color.DarkGreen));
                    }
                    else
                    {
                        await context.Channel.SendMessageAsync(
                            embed: EmbedFormats.CreateErrorEmbed("**An error occurred!** Please try again."));
                    }
                    break;

                //If user wants to setup Role on Join
                case "Role on Join":

                    await context.Channel.SendMessageAsync(
                        "Please type the name of the role you want to setup as the role on join:");
                    var roleResult = await _interactivityService.NextMessageAsync(x => x.Author == context.User);

                    if (roleResult.IsSuccess)
                    {
                        var role = Helper.DoesRoleExist(context.Guild, roleResult.Value.Content);

                        if (role == null)
                        {
                            await context.Channel.SendMessageAsync(
                                embed: EmbedFormats.CreateErrorEmbed("**Role doesn't exist!** Please try again."));
                            return;
                        }

                        var guildSettings = await _botContext.GetGuildSettings(context.Guild.Id);

                        guildSettings.RoleOnJoin = role.Name;
                        await _botContext.SaveChangesAndExpireAsync(context.Guild.Id.ToString());
                        await context.Channel.SendMessageAsync(embed: EmbedFormats.CreateBasicEmbed(
                            "Role on join changed", $"Role on join was set to **{role.Name}**",
                            Color.DarkGreen));
                    }
                    else
                    {
                        await context.Channel.SendMessageAsync(
                            embed: EmbedFormats.CreateErrorEmbed("**An error occurred!** Please try again."));
                    }
                    break;

                //If user wants to setup Custom Prefix
                case "Custom Prefix":

                    await context.Channel.SendMessageAsync(
                        "Please type the new prefix for your guild (type `default` to reset it):");
                    var prefixResult = await _interactivityService.NextMessageAsync(x => x.Author == context.User);

                    if (prefixResult.IsSuccess)
                    {
                        var guildSettings = await _botContext.GetGuildSettings(context.Guild.Id);

                        var newPrefix = prefixResult.Value.Content;

                        //If newPrefix == default, it means the user wants to reset the prefix
                        if (newPrefix.ToLower() == "default")
                        {
                            //We check if the current prefix is already the default one (null)
                            string currentPrefix = guildSettings.CustomPrefix;
                            if (currentPrefix == null)
                            {
                                await context.Channel.SendMessageAsync(
                                    embed: EmbedFormats.CreateErrorEmbed("Bot prefix is already the default one!"));
                                return;
                            }

                            //If the current prefix isn't the default, then set it to null (default)
                            guildSettings.CustomPrefix = null;
                            await _botContext.SaveChangesAndExpireAsync(context.Guild.Id.ToString());
                            await context.Channel.SendMessageAsync(
                                embed: EmbedFormats.CreateBasicEmbed("Custom prefix Changed", "Bot prefix was reset to **-**",
                                    Color.DarkGreen));
                            return;
                        }

                        //If the newPrefix is larger than 5 characters, return
                        if (newPrefix.Length > 5)
                        {
                            await context.Channel.SendMessageAsync(
                                embed: EmbedFormats.CreateErrorEmbed("Bot prefix can't be longer than 5 characters!"));
                            return;
                        }

                        //If all checks pass, set the newPrefix as the guilds custom prefix
                        guildSettings.CustomPrefix = newPrefix;
                        await _botContext.SaveChangesAndExpireAsync(context.Guild.Id.ToString());
                        await context.Channel.SendMessageAsync(embed: EmbedFormats.CreateBasicEmbed(
                            "Custom prefix Changed", $"Cobra's prefix is now:  **{newPrefix}**", Color.DarkGreen));
                    }
                    else
                    {
                        await context.Channel.SendMessageAsync(
                            embed: EmbedFormats.CreateErrorEmbed("**An error occurred!** Please try again."));
                    }
                    break;

                //If user wants to setup Moderation Channel
                case "Moderation Channel":
                    await context.Channel.SendMessageAsync(
                        "Please mention the #textChannel you want to setup as the Moderation Channel:");

                    var moderationResult = await _interactivityService.NextMessageAsync(x => x.Author == context.User);

                    if (moderationResult.IsSuccess)
                    {
                        var textChannel = (ITextChannel)moderationResult.Value.MentionedChannels.FirstOrDefault();

                        if (textChannel == null)
                        {
                            await context.Channel.SendMessageAsync(
                                embed: EmbedFormats.CreateErrorEmbed("**No channel specified!** Please try again."));
                            return;
                        }

                        var guildSettings = await _botContext.GetGuildSettings(context.Guild.Id);

                        guildSettings.ModerationChannel = textChannel.Id;
                        await _botContext.SaveChangesAndExpireAsync(context.Guild.Id.ToString());
                        await context.Channel.SendMessageAsync(embed: EmbedFormats.CreateBasicEmbed(
                            "Moderation channel changed", $"Welcome channel is now {textChannel.Mention}",
                            Color.DarkGreen));
                    }
                    else
                    {
                        await context.Channel.SendMessageAsync(
                            embed: EmbedFormats.CreateErrorEmbed("**An error occurred!** Please try again."));
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
                    return EmbedFormats.CreateErrorEmbed("Bot prefix is already the default one!");


                //If they have a custom prefix, set it to null
                guildSettings.CustomPrefix = null;
                await _botContext.SaveChangesAndExpireAsync(context.Guild.Id.ToString());
                return EmbedFormats.CreateBasicEmbed("", "Bot prefix was reset to:  **-**", Color.DarkGreen);
            }

            //If user input is longer than 5, return
            if (prefix.Length > 5)
                return EmbedFormats.CreateErrorEmbed("Bot prefix can't be longer than 5 characters!");

            //If every check passes, we add the new custom prefix to the database
            guildSettings.CustomPrefix = prefix;
            await _botContext.SaveChangesAndExpireAsync(context.Guild.Id.ToString());

            return EmbedFormats.CreateBasicEmbed("Custom prefix Changed", $"Cobra's prefix is now:  **{prefix}**", Color.DarkGreen);
        }

        /// <summary>Sets guild's welcome channel.
        /// </summary>
        public async Task<Embed> SetWelcomeChannel(ITextChannel textChannel)
        {
            var guildSettings = await _botContext.GetGuildSettings(textChannel.Guild.Id);

            guildSettings.WelcomeChannel = textChannel.Id;
            await _botContext.SaveChangesAndExpireAsync(textChannel.Guild.Id.ToString());

            return EmbedFormats.CreateBasicEmbed("Welcome channel changed", $"Welcome channel is now {textChannel.Mention}", Color.DarkGreen);
        }

        /// <summary>Resets guild's welcome channel.
        /// </summary>
        public async Task<Embed> ResetWelcomeChannel(SocketCommandContext context)
        {
            var guildSettings = await _botContext.GetGuildSettings(context.Guild.Id);

            guildSettings.WelcomeChannel = 0;
            await _botContext.SaveChangesAndExpireAsync(context.Guild.Id.ToString());

            return EmbedFormats.CreateBasicEmbed("Welcome channel changed",
                "Welcome channel was reset.\nYour server doesn't have a welcome channel setup right now.",
                Color.DarkMagenta);
        }

        /// <summary>Changes role that users receive when they join the server.
        /// </summary>
        public async Task<Embed> SetRoleOnJoin(IGuild guild, string roleName)
        {
            var role = Helper.DoesRoleExist(guild, roleName);

            if (role == null)
                return EmbedFormats.CreateErrorEmbed($"Role **{roleName}** doesn't exist!");

            var guildSettings = await _botContext.GetGuildSettings(guild.Id);

            guildSettings.RoleOnJoin = role.Name;
            await _botContext.SaveChangesAndExpireAsync(guild.Id.ToString());

            return EmbedFormats.CreateBasicEmbed("Role on join changed", $"Role on join was set to **{role.Name}**", Color.DarkGreen);
        }

        /// <summary>Changes role that users receive when they join the server.
        /// </summary>
        public async Task<Embed> ResetRoleOnJoin(SocketCommandContext context)
        {
            var guildSettings = await _botContext.GetGuildSettings(context.Guild.Id);

            guildSettings.RoleOnJoin = null;
            await _botContext.SaveChangesAndExpireAsync(context.Guild.Id.ToString());

            return EmbedFormats.CreateBasicEmbed("Role on join changed",
                "Role on join was reset.\nYour server doesn't have a role on join setup right now.", Color.DarkMagenta);
        }
    }
}
