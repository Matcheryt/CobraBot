using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using CobraBot.Common;
using CobraBot.Database;
using Discord;
using Discord.Commands;
using Discord.Net;
using Interactivity;
using Microsoft.EntityFrameworkCore;
using Z.EntityFramework.Plus;

namespace CobraBot.Services
{
    public sealed class InfoService
    {
        private readonly BotContext _botContext;
        private readonly CommandService _commandService;
        private readonly InteractivityService _interactivityService;
        private readonly IServiceProvider _serviceProvider;

        public InfoService(BotContext botContext, CommandService commandService, IServiceProvider serviceProvider, InteractivityService interactivityService)
        {
            _botContext = botContext;
            _commandService = commandService;
            _serviceProvider = serviceProvider;
            _interactivityService = interactivityService;
        }

        /// <summary>Send an embed with current server information.
        /// </summary>
        public async Task ServerInfoAsync(SocketCommandContext context)
        {
            //Get guilds custom prefix.
            //Sets prefix to - if the guild doesn't have a custom prefix
            var prefix = _botContext.Guilds.AsNoTracking().Where(x => x.GuildId == context.Guild.Id)
                .FromCache(context.Guild.Id.ToString()).FirstOrDefault()?.CustomPrefix ?? "-";
            
            var memberCount = context.Guild.MemberCount;
            var serverId = context.Guild.Id;
            var serverName = context.Guild.Name;
            var serverOwner = context.Guild.Owner;
            var serverRegion = context.Guild.VoiceRegionId;

            EmbedFieldBuilder[] fields =
            {
                new EmbedFieldBuilder().WithName("Bot prefix:").WithValue($"`{prefix}`").WithIsInline(true),
                new EmbedFieldBuilder().WithName("Help command:").WithValue($"`{prefix}help`").WithIsInline(true),
                new EmbedFieldBuilder().WithName("Server name:").WithValue(serverName),
                new EmbedFieldBuilder().WithName("Server owner:").WithValue(serverOwner).WithIsInline(true),
                new EmbedFieldBuilder().WithName("Member count:").WithValue(memberCount).WithIsInline(true),
                new EmbedFieldBuilder().WithName("Server ID:").WithValue(serverId),
                new EmbedFieldBuilder().WithName("Server region:").WithValue(serverRegion).WithIsInline(true)
            };

            await context.Channel.SendMessageAsync(
                embed: EmbedFormats.CreateInfoEmbed($"{serverName} info", context.Guild.Description,
                    new EmbedFooterBuilder().WithIconUrl(context.User.GetAvatarUrl())
                        .WithText($"Requested by: {context.User}"), context.Guild.IconUrl, fields));
        }

        /// <summary>Returns discord user info.
        /// </summary>
        public static Embed ShowUserInfoAsync(IGuildUser user)
        {
            if (user == null)
                return EmbedFormats.CreateErrorEmbed("**Please specify a user**");

            var thumbnailUrl = user.GetAvatarUrl();
            var accountCreationDate = $"{user.CreatedAt.Day}/{user.CreatedAt.Month}/{user.CreatedAt.Year}";
            var joinedAt = $"{user.JoinedAt.Value.Day}/{user.JoinedAt.Value.Month}/{user.JoinedAt.Value.Year}";
            var username = user.Username;
            var discriminator = user.Discriminator;
            var id = user.Id;
            var status = user.Status;
            var game = user.Activity;

            var author = new EmbedAuthorBuilder()
            {
                Name = user.Username + " info",
                IconUrl = thumbnailUrl,
            };

            var usernameField = new EmbedFieldBuilder().WithName("Username").WithValue(username ?? "_Not found_").WithIsInline(true);
            var discriminatorField = new EmbedFieldBuilder().WithName("Discriminator").WithValue(discriminator ?? "_Not found_").WithIsInline(true);
            var userIdField = new EmbedFieldBuilder().WithName("User ID").WithValue(id).WithIsInline(true);
            var createdAtField = new EmbedFieldBuilder().WithName("Created At").WithValue(accountCreationDate).WithIsInline(true);
            var currentStatusField = new EmbedFieldBuilder().WithName("Current Status").WithValue(status).WithIsInline(true);
            var joinedAtField = new EmbedFieldBuilder().WithName("Joined Server At").WithValue(joinedAt).WithIsInline(true);
            var playingField = new EmbedFieldBuilder().WithName("Playing").WithValue((object)game ?? "_Not found_").WithIsInline(true);

            var embed = new EmbedBuilder()
                .WithColor(Color.DarkGreen)
                .WithAuthor(author)
                .WithThumbnailUrl(thumbnailUrl)
                .WithFields(usernameField, discriminatorField, userIdField, currentStatusField, createdAtField, joinedAtField, playingField);

            return embed.Build();
        }

        /// <summary>Send an embed with commands available.
        /// </summary>
        public async Task HelpAsync(SocketCommandContext context)
        {
            //Get guilds custom prefix.
            //Sets prefix to - if the guild doesn't have a custom prefix
            var prefix = _botContext.Guilds.AsNoTracking().Where(x => x.GuildId == context.Guild.Id)
                             .FromCache(context.Guild.Id.ToString()).FirstOrDefault()?.CustomPrefix ?? "-";

            var helpEmbed = new EmbedBuilder()
                .WithColor(Color.DarkGreen)
                .WithAuthor(new EmbedAuthorBuilder().WithIconUrl(context.Guild.IconUrl)
                    .WithName($"Commands you have access to on {context.Guild.Name}"))
                .WithDescription($"For help with a specific command type  `{prefix}help [command]`")
                .WithFooter(x =>
                {
                    x.Text = "Cobra | cobra.telmoduarte.me";
                    x.IconUrl = context.Client.CurrentUser.GetAvatarUrl();
                });

            foreach (var module in _commandService.Modules)
            {
                var description = new StringBuilder();
                
                //Itterate through every command in current module
                foreach (var command in module.Commands)
                {
                    //Check if user has permission to execute said command
                    var result = await command.CheckPreconditionsAsync(context, _serviceProvider);
                    
                    //If the user does have permission, then continue
                    if (!result.IsSuccess) continue;

                    //Append command parameters if the command has them
                    if (command.Parameters.Any())
                    {
                        //Append the command to the description string builder
                        description.Append($"`{prefix}{command.Aliases[0]} ");
                        //Append command parameters if the command has them
                        description.Append($"[{string.Join(", ", command.Parameters.Select(p => p.Name))}]`");
                    }
                    else
                    {
                        //Append the command to the description string builder
                        description.Append($"`{prefix}{command.Aliases[0]}`");
                    }
                   
                    //Append new line so commands don't get mixed up in one line
                    description.Append('\n');
                }
                
                //If description isn't null or white space
                if (!string.IsNullOrWhiteSpace(description.ToString()))
                {
                    //Then we create a field for the current module, with it's value being the commands from the current module
                    helpEmbed.AddField(x =>
                    {
                        x.Name = module.Name;
                        x.Value = description.ToString();
                        x.IsInline = false;
                    });
                }
            }

            try
            {
                //Sends DM to the user with the commands
                await context.User.SendMessageAsync(embed: helpEmbed.Build());
            }
            catch(HttpException)
            {
                //If an exception throws, chances is that that exception is because of the user not having DM's enabled
                //So we inform the user about it
                await context.Channel.SendMessageAsync(embed: EmbedFormats.CreateErrorEmbed("**I can't send you DM's!**\nPlease enable DM's in your privacy settings."));
            }
        }

        /// <summary>Send an embed with info about specified command.
        /// </summary>
        public async Task HelpAsync(SocketCommandContext context, string commandName)
        {
            //Get guilds custom prefix.
            //Sets prefix to - if the guild doesn't have a custom prefix
            var prefix = _botContext.Guilds.AsNoTracking().Where(x => x.GuildId == context.Guild.Id)
                .FromCache(context.Guild.Id.ToString()).FirstOrDefault()?.CustomPrefix ?? "-";
            
            //Search for commands equal to commandName
            var searchResult = _commandService.Search(context, commandName);

            //If no commands are found
            if (!searchResult.IsSuccess)
            {
                _interactivityService.DelayedSendMessageAndDeleteAsync(context.Channel, null, TimeSpan.FromSeconds(5), null, false,
                    EmbedFormats.CreateErrorEmbed($"**Unknown command: `{commandName}`**"));
                return;
            }

            //If there is a match, then get the first match
            var commandMatch = searchResult.Commands[0];

            //Get the command info and command parameters
            var cmd = commandMatch.Command;
            var param = cmd.Parameters.Select(x => x.Name);

            //Send message on how to use the command
            var helpEmbed = new EmbedBuilder()
                .WithColor(Color.DarkGreen)
                .WithTitle($"Command: {cmd.Aliases[0]}")
                .WithDescription(param.Any()
                ? $"**Description:** {cmd.Summary}\n**Usage:** `{prefix}{cmd.Aliases[0]} [{string.Join(", ", param)}]`"
                : $"**Description:** {cmd.Summary}\n**Usage:** `{prefix}{cmd.Aliases[0]}`")
                .WithFooter($"Aliases: {string.Join(", ", cmd.Aliases)}");

            await context.Channel.SendMessageAsync(embed: helpEmbed.Build());
        }

        /// <summary>Send an embed with the bot uptime.
        /// </summary>
        public static async Task GetBotInfoAsync(SocketCommandContext context)
        {
            var uptime = DateTime.Now.Subtract(Process.GetCurrentProcess().StartTime);
            var uptimeString = $"{uptime.Days} days, {uptime.Hours} hours and {uptime.Minutes} minutes";
            
            var heapSize = Math.Round(GC.GetTotalMemory(true) / (1024.0 * 1024.0), 2)
                .ToString(CultureInfo.CurrentCulture);

            EmbedFieldBuilder[] fields =
            {
                new EmbedFieldBuilder().WithName("Uptime:").WithValue(uptimeString),
                new EmbedFieldBuilder().WithName("Discord.NET version:")
                    .WithValue(DiscordConfig.Version),
                new EmbedFieldBuilder().WithName("Heap size:").WithValue($"{heapSize} MB").WithIsInline(true),
                new EmbedFieldBuilder().WithName("Environment:")
                    .WithValue($"{RuntimeInformation.FrameworkDescription} {RuntimeInformation.OSArchitecture}")
                    .WithIsInline(true),
                new EmbedFieldBuilder().WithName("Guilds:").WithValue(context.Client.Guilds.Count),
                new EmbedFieldBuilder().WithName("Users:").WithValue(context.Client.Guilds.Sum(x => x.MemberCount))
                    .WithIsInline(true),
                new EmbedFieldBuilder().WithName("Vote:")
                    .WithValue("[Vote here](https://top.gg/bot/389534436099883008/vote)")
            };
            
            await context.Channel.SendMessageAsync(embed: EmbedFormats.CreateInfoEmbed(
                $"Cobra v{Assembly.GetEntryAssembly()?.GetName().Version?.ToString(2)}","",
                new EmbedFooterBuilder().WithText("Developed by Matcher#0183"), context.Client.CurrentUser.GetAvatarUrl(), fields));
        }
    }
}