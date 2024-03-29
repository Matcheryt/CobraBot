﻿/*
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
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using CobraBot.Common.EmbedFormats;
using CobraBot.Common.Extensions;
using CobraBot.Database;
using Discord;
using Discord.Commands;
using Discord.Net;
using Interactivity;

namespace CobraBot.Services
{
    public sealed class InfoService
    {
        private readonly BotContext _botContext;
        private readonly CommandService _commandService;
        private readonly InteractivityService _interactivityService;
        private readonly IServiceProvider _serviceProvider;

        public InfoService(BotContext botContext, CommandService commandService, IServiceProvider serviceProvider,
            InteractivityService interactivityService)
        {
            _botContext = botContext;
            _commandService = commandService;
            _serviceProvider = serviceProvider;
            _interactivityService = interactivityService;
        }

        /// <summary> Send an embed with current server information. </summary>
        public async Task ServerInfoAsync(SocketCommandContext context)
        {
            //Get guilds custom prefix.
            //Sets prefix to - if the guild doesn't have a custom prefix
            var prefix = _botContext.GetGuildPrefix(context.Guild.Id);

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
                embed: CustomFormats.CreateInfoEmbed($"{serverName} info", context.Guild.Description,
                    new EmbedFooterBuilder().WithIconUrl(context.User.GetAvatarUrl())
                        .WithText($"Requested by: {context.User}"), context.Guild.IconUrl, fields));
        }


        /// <summary> Returns discord user info. </summary>
        public static Embed ShowUserInfoAsync(IUser user)
        {
            var guildUser = (IGuildUser)user;
            var joinedGuildAt = guildUser.JoinedAt?.UtcDateTime.ToShortDateString() ?? "_Not found_";

            var thumbnailUrl = user.GetAvatarUrl();
            var accountCreationDate = $"{user.CreatedAt.UtcDateTime}";
            var username = user.Username;
            var discriminator = user.Discriminator;
            var id = user.Id;
            var status = user.Status;
            var activities = user.Activities;

            var author = new EmbedAuthorBuilder
            {
                Name = user.Username + " info",
                IconUrl = thumbnailUrl
            };

            var usernameField = new EmbedFieldBuilder().WithName("Username").WithValue(username ?? "_Not found_")
                .WithIsInline(true);
            var discriminatorField = new EmbedFieldBuilder().WithName("Discriminator")
                .WithValue(discriminator ?? "_Not found_").WithIsInline(true);
            var userIdField = new EmbedFieldBuilder().WithName("User ID").WithValue(id).WithIsInline(true);
            var createdAtField = new EmbedFieldBuilder().WithName("Created At").WithValue(accountCreationDate)
                .WithIsInline(true);
            var currentStatusField =
                new EmbedFieldBuilder().WithName("Current Status").WithValue(status).WithIsInline(true);
            var joinedAtField = new EmbedFieldBuilder().WithName("Joined Server At").WithValue(joinedGuildAt)
                .WithIsInline(true);

            //TODO: Properly handle activities
            var activitiesField = new EmbedFieldBuilder().WithName("Activity")
                .WithValue(activities.FirstOrDefault()?.Name ?? "_No activity_").WithIsInline(true);

            var embed = new EmbedBuilder()
                .WithColor(0x268618)
                .WithAuthor(author)
                .WithThumbnailUrl(thumbnailUrl)
                .WithFooter("Note: Dates are in shown in UTC time zone")
                .WithFields(usernameField, discriminatorField, userIdField, currentStatusField, createdAtField,
                    joinedAtField, activitiesField);

            return embed.Build();
        }


        /// <summary> Send an embed with commands available. </summary>
        public async Task HelpAsync(SocketCommandContext context)
        {
            //Get guilds custom prefix.
            //Sets prefix to - if the guild doesn't have a custom prefix
            var prefix = _botContext.GetGuildPrefix(context.Guild.Id);

            var helpEmbed = new EmbedBuilder()
                .WithColor(0x268618)
                .WithAuthor(new EmbedAuthorBuilder().WithIconUrl(context.Guild.IconUrl)
                    .WithName($"Commands you have access to on {context.Guild.Name}"))
                .WithDescription(
                    $"The prefix for commands is `{prefix}`\nYou can also @mention me to execute commands!\nFor help with a specific command type  `{prefix}chelp [command]`")
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
                    var canExecute = await command.HasPermissionToExecute(context, _serviceProvider);

                    if (!canExecute)
                        continue;

                    description.Append($"`{command.Aliases[0]}`");

                    //Append new line so commands don't get mixed up in one line
                    description.Append(' ');
                }

                //If description isn't null or white space
                if (!string.IsNullOrWhiteSpace(description.ToString()))
                    //Then we create a field for the current module, with it's value being the commands from the current module
                    helpEmbed.AddField(x =>
                    {
                        x.Name = module.Name;
                        x.Value = description.ToString();
                        x.IsInline = false;
                    });
            }

            try
            {
                //Sends DM to the user with the commands
                await context.User.SendMessageAsync(embed: helpEmbed.Build());
            }
            catch (HttpException)
            {
                //If an exception throws, chances is that that exception is because of the user not having DM's enabled
                //So we inform the user about it
                await context.Channel.SendMessageAsync(
                    embed: CustomFormats.CreateErrorEmbed(
                        "**I can't send you DM's!**\nPlease enable DM's in your privacy settings."));
            }
        }


        /// <summary> Send an embed with info about specified command. </summary>
        public async Task HelpAsync(SocketCommandContext context, string commandName)
        {
            //Get guilds custom prefix.
            //Sets prefix to - if the guild doesn't have a custom prefix
            var prefix = _botContext.GetGuildPrefix(context.Guild.Id);

            //Search for commands equal to commandName
            var searchResult = _commandService.Search(context, commandName);

            //If no commands are found
            if (!searchResult.IsSuccess)
            {
                _interactivityService.DelayedSendMessageAndDeleteAsync(context.Channel, null, TimeSpan.FromSeconds(5),
                    null, false,
                    CustomFormats.CreateErrorEmbed($"**Unknown command: `{commandName}`**"));
                return;
            }

            //If there is a match, then get the command which matches the command specified
            var cmd = searchResult.Commands.First(x => x.Alias.Contains(commandName)).Command;

            //Check if user has permission to execute said command
            var canExecute = await cmd.HasPermissionToExecute(context, _serviceProvider);

            if (!canExecute)
            {
                await context.Channel.SendMessageAsync(
                    embed: CustomFormats.CreateErrorEmbed("You don't have permission for that command!"));
                return;
            }

            //Get command parameters
            var parameters = cmd.Parameters.ToArray();

            //True if the command has parameters, false if the command doesn't
            var hasParameters = parameters.Any();

            var usageBuilder = new StringBuilder();

            //If the command has parameters, we build a usage string
            if (hasParameters)
            {
                foreach (var parameter in parameters)
                {
                    //If the current parameter is optional, then we append <parameter> so the user knows it is optional
                    if (parameter.IsOptional)
                    {
                        usageBuilder.Append($"<{parameter.Name}> ");
                        continue;
                    }

                    //If it isnt optional, we append [parameter] so the user knows that the parameter is required
                    usageBuilder.Append($"[{parameter.Name}] ");
                }

                //Removes the last char of the string, because when building the string a blank space is always left at the end
                usageBuilder.Remove(usageBuilder.Length - 1, 1);
            }

            //Send message on how to use the command
            var helpEmbed = new EmbedBuilder()
                .WithColor(0x268618)
                .WithTitle($"Command: {cmd.Aliases[0]}")
                .WithDescription(hasParameters
                    //If command has parameters
                    ? $"**Aliases:** `{string.Join(", ", cmd.Aliases)}`\n" +
                      $"**Description:** {cmd.Summary}\n" +
                      $"**Usage:** `{prefix}{cmd.Aliases[0]} {usageBuilder}`"

                    //If command doesn't have parameters
                    : $"**Aliases:** `{string.Join(", ", cmd.Aliases)}`\n" +
                      $"**Description:** {cmd.Summary}\n" +
                      $"**Usage:** `{prefix}{cmd.Aliases[0]}`")
                .WithFooter(hasParameters ? "Parameters inside <angle brackets> are optional." : "");

            await context.Channel.SendMessageAsync(embed: helpEmbed.Build());
        }


        /// <summary> Send an embed with the bot uptime. </summary>
        public static async Task GetBotInfoAsync(SocketCommandContext context)
        {
            var uptime = DateTime.Now.Subtract(Process.GetCurrentProcess().StartTime);
            var uptimeString = $"{uptime.Days} days, {uptime.Hours} hours and {uptime.Minutes} minutes";

            var heapSize = Math.Round(GC.GetTotalMemory(true) / (1024.0 * 1024.0), 2)
                .ToString(CultureInfo.CurrentCulture);

            EmbedFieldBuilder[] fields =
            {
                new EmbedFieldBuilder().WithName("Uptime").WithValue(uptimeString),
                new EmbedFieldBuilder().WithName("Discord.NET version")
                    .WithValue(DiscordConfig.Version),
                new EmbedFieldBuilder().WithName("Heap size").WithValue($"{heapSize} MB").WithIsInline(true),
                new EmbedFieldBuilder().WithName("Environment")
                    .WithValue($"{RuntimeInformation.FrameworkDescription} {RuntimeInformation.OSArchitecture}")
                    .WithIsInline(true),
                new EmbedFieldBuilder().WithName("Guilds").WithValue(context.Client.Guilds.Count),
                new EmbedFieldBuilder().WithName("Users").WithValue(context.Client.Guilds.Sum(x => x.MemberCount))
                    .WithIsInline(true),
                new EmbedFieldBuilder().WithName("Vote")
                    .WithValue(
                        "[Top.gg](https://top.gg/bot/389534436099883008/vote)\n[Discord Bot List](https://discordbotlist.com/bots/cobra/upvote)")
            };

            await context.Channel.SendMessageAsync(embed: CustomFormats.CreateInfoEmbed(
                $"Cobra v{Assembly.GetEntryAssembly()?.GetName().Version?.ToString(2)}", "",
                new EmbedFooterBuilder().WithText("Developed by Matcher#0183"),
                context.Client.CurrentUser.GetAvatarUrl(), fields));
        }


        /// <summary> Send an embed with the bot latency. </summary>
        public static async Task LatencyAsync(SocketCommandContext context)
        {
            var clientLatency = (DateTimeOffset.UtcNow - context.Message.CreatedAt).Milliseconds;
            var websocketLatency = context.Client.Latency;

            var embed = new EmbedBuilder()
                .WithTitle("Latency")
                .WithColor(0x268618)
                .AddField(x =>
                {
                    x.Name = "Client latency";
                    x.Value = $"`{clientLatency}ms`";
                })
                .AddField(x =>
                {
                    x.Name = "Websocket latency";
                    x.Value = $"`{websocketLatency}ms`";
                })
                .WithCurrentTimestamp()
                .Build();

            await context.Channel.SendMessageAsync(embed: embed);
        }


        /// <summary> Shows Cobra's invitation link </summary>
        public static async Task InviteAsync(SocketCommandContext context)
        {
            var inviteEmbed = new EmbedBuilder()
                .WithColor(0x268618)
                .WithFooter(x =>
                {
                    x.IconUrl = context.Client.CurrentUser.GetAvatarUrl();
                    x.Text = "cobra.telmoduarte.me";
                })
                .WithTitle("📫  Invite Cobra")
                .AddField(x =>
                {
                    x.Name = "Add Cobra to your server!";
                    x.Value =
                        "[Click here](https://discord.com/api/oauth2/authorize?client_id=389534436099883008&permissions=8&redirect_uri=https%3A%2F%2Fdiscordapp.com%2F&scope=bot)";
                })
                .Build();

            await context.Channel.SendMessageAsync(embed: inviteEmbed);
        }
    }
}