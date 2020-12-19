using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CobraBot.Common;
using CobraBot.Database;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Z.EntityFramework.Plus;

namespace CobraBot.Services
{
    public sealed class InfoService
    {
        private readonly BotContext _botContext;

        public InfoService(BotContext botContext)
        {
            _botContext = botContext;
        }

        /// <summary>Send an embed with current server information.
        /// </summary>
        public async Task ServerInfoAsync(SocketCommandContext context)
        {
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

            await context.Channel.SendMessageAsync(embed: EmbedFormats.CreateInfoEmbed($"{serverName} info", context.Guild.IconUrl, fields));
        }

        public static async Task GetUptimeAsync(SocketCommandContext context)
        {
            var uptime = DateTime.Now.Subtract(Process.GetCurrentProcess().StartTime);
            var uptimeString = $"{uptime.Days} days, {uptime.Hours} hours and {uptime.Minutes} minutes";

            EmbedFieldBuilder[] fields =
            {
                new EmbedFieldBuilder().WithName("Uptime:").WithValue(uptimeString),
                new EmbedFieldBuilder().WithName("Dotnet version:").WithValue(Environment.Version).WithIsInline(true),
                new EmbedFieldBuilder().WithName("Discord.NET version:")
                    .WithValue(Assembly.GetAssembly(typeof(DiscordSocketClient))?.GetName().Version?.ToString())
                    .WithIsInline(true),
                new EmbedFieldBuilder().WithName("Vote:")
                    .WithValue("[Vote here](https://top.gg/bot/389534436099883008/vote)")
            };

            await context.Channel.SendMessageAsync(embed: EmbedFormats.CreateInfoEmbed(
                $"Cobra v{Assembly.GetEntryAssembly()?.GetName().Version?.ToString(fieldCount: 2)}",
                context.Client.CurrentUser.GetAvatarUrl(), fields));
        }
    }
}
