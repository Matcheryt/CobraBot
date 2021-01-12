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
                    Color.DarkGreen));
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
                                embed: CustomFormats.CreateErrorEmbed("**No channel specified!** Please try again."));
                            return;
                        }

                        await context.Channel.SendMessageAsync(embed: await SetWelcomeChannel(textChannel));
                    }
                    break;

                //If user wants to setup Role on Join
                case "Role on Join":

                    await context.Channel.SendMessageAsync(
                        "Please type the name of the role you want to setup as the role on join:");
                    var roleResult = await _interactivityService.NextMessageAsync(x => x.Author == context.User);

                    if (roleResult.IsSuccess)
                    {
                        await context.Channel.SendMessageAsync(embed: await SetRoleOnJoin(context.Guild, roleResult.Value.Content));
                    }
                    break;

                //If user wants to setup Custom Prefix
                case "Custom Prefix":

                    await context.Channel.SendMessageAsync(
                        "Please type the new prefix for your guild (type `default` to reset it):");
                    var prefixResult = await _interactivityService.NextMessageAsync(x => x.Author == context.User);

                    if (prefixResult.IsSuccess)
                    {
                        await context.Channel.SendMessageAsync(embed: await ChangePrefixAsync(prefixResult.Value.Content, context));
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
                                embed: CustomFormats.CreateErrorEmbed("**No channel specified!** Please try again."));
                            return;
                        }

                        var guildSettings = await _botContext.GetGuildSettings(context.Guild.Id);

                        guildSettings.ModerationChannel = textChannel.Id;
                        await _botContext.SaveChangesAsync();
                        await context.Channel.SendMessageAsync(embed: CustomFormats.CreateBasicEmbed(
                            "Moderation channel changed", $"Moderation channel is now {textChannel.Mention}",
                            Color.DarkGreen));
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
                return CustomFormats.CreateBasicEmbed("", "Bot prefix was reset to:  **-**", Color.DarkGreen);
            }

            //If user input is longer than 5, return
            if (prefix.Length > 5)
                return CustomFormats.CreateErrorEmbed("Bot prefix can't be longer than 5 characters!");

            //If every check passes, we add the new custom prefix to the database
            guildSettings.CustomPrefix = prefix;
            await _botContext.SaveChangesAsync();

            return CustomFormats.CreateBasicEmbed("Custom prefix Changed", $"Cobra's prefix is now:  **{prefix}**", Color.DarkGreen);
        }

        #region Welcome
        /// <summary>Sets guild's welcome channel.
        /// </summary>
        public async Task<Embed> SetWelcomeChannel(ITextChannel textChannel)
        {
            var guildSettings = await _botContext.GetGuildSettings(textChannel.Guild.Id);

            guildSettings.WelcomeChannel = textChannel.Id;
            await _botContext.SaveChangesAsync();

            return CustomFormats.CreateBasicEmbed("Welcome channel changed", $"Welcome channel is now {textChannel.Mention}", Color.DarkGreen);
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
        public async Task<Embed> SetRoleOnJoin(IGuild guild, string roleName)
        {
            var role = Helper.DoesRoleExist(guild, roleName);

            if (role == null)
                return CustomFormats.CreateErrorEmbed($"Role **{roleName}** doesn't exist!");

            var guildSettings = await _botContext.GetGuildSettings(guild.Id);

            guildSettings.RoleOnJoin = role.Name;
            await _botContext.SaveChangesAsync();

            return CustomFormats.CreateBasicEmbed("Role on join changed", $"Role on join was set to **{role.Name}**", Color.DarkGreen);
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