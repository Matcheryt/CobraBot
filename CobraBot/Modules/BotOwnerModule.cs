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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CobraBot.Database;
using CobraBot.Handlers;
using CobraBot.Helpers;
using Discord;
using Discord.Commands;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace CobraBot.Modules
{
    [RequireOwner]
    [Name("Owner")]
    public class BotOwnerModule : ModuleBase<SocketCommandContext>
    {
        public IHost Host { get; set; }
        public BotContext BotContext { get; set; }

        //Defines bot's status
        [Command("setbotgame")]
        public async Task SetGame(string status, string activity = null, string url = null)
        {
            var activityType = activity switch
            {
                "streaming" => ActivityType.Streaming,
                "playing" => ActivityType.Playing,
                "listening" => ActivityType.Listening,
                "watching" => ActivityType.Watching,
                "custom" => ActivityType.CustomStatus,
                _ => ActivityType.Playing
            };

            await Context.Client.SetGameAsync(status, url, activityType);
            Console.WriteLine(
                $"{DateTime.Now}: Cobra's status was changed to {status} with activity type: {activityType}");
        }


        //Downloads users from every guild to cache
        [Command("downloadusers")]
        public async Task DownloadUsers()
        {
            await Context.Client.DownloadUsersAsync(Context.Client.Guilds);
            await Context.Message.AddReactionAsync(new Emoji("👍"));
        }


        //Leave specified guild
        [Command("leaveguild")]
        public async Task LeaveGuild(ulong guildId)
        {
            await Context.Client.GetGuild(guildId).LeaveAsync();
            await Context.Message.AddReactionAsync(new Emoji("👍"));
        }

        //Removes db entries from guilds where the bot isn't joined anymore
        [Command("dbclean")]
        public async Task DbClean()
        {
            var guildIds = Context.Client.Guilds.Select(x => x.Id).ToList();
            var dbGuilds = BotContext.Guilds;

            uint removedGuilds = 0;

            foreach (var guild in dbGuilds)
            {
                if (guildIds.Any(x => x == guild.GuildId)) continue;

                BotContext.Remove(guild);
                await BotContext.SaveChangesAsync();
                removedGuilds++;
            }

            if (removedGuilds == 0)
            {
                await ReplyAsync("No guilds were removed.");
                return;
            }

            Log.Information($"Removed {removedGuilds} from the database.");
            await ReplyAsync($"Removed {removedGuilds} from the database.");
        }


        //Update discord bot lists
        [Command("updatebotlists")]
        public async Task UpdateBotLists()
        {
            var serverCount = Context.Client.Guilds.Count;
            var usersCount = Context.Client.Guilds.Sum(x => x.MemberCount);

            var dblApiUrl = $"https://discordbotlist.com/api/v1/bots/{Context.Client.CurrentUser.Id}/stats";
            var topApiUrl = $"https://top.gg/api/bots/{Context.Client.CurrentUser.Id}/stats";

            var httpClient = HttpHelper.HttpClient;

            //Discord bot list
            var dblContent = new Dictionary<string, string>
            {
                { "users", $"{usersCount}" },
                { "guilds", $"{serverCount}" }
            };

            //Top.gg bot list
            var topContent = new StringContent($"{{\"server_count\":\"{serverCount}\"}}", Encoding.UTF8,
                "application/json");


            //Requests
            var dblRequest = new HttpRequestMessage
            {
                RequestUri = new Uri(dblApiUrl),
                Method = HttpMethod.Post,
                Headers =
                {
                    { "Authorization", Configuration.DblApiKey }
                },
                Content = new FormUrlEncodedContent(dblContent)
            };

            var topRequest = new HttpRequestMessage
            {
                RequestUri = new Uri(topApiUrl),
                Method = HttpMethod.Post,
                Headers =
                {
                    { "Authorization", Configuration.TopggApiKey }
                },
                Content = topContent
            };


            //Send requests
            await httpClient.SendAsync(dblRequest);
            await httpClient.SendAsync(topRequest);

            Log.Information($"Bot lists updated with {serverCount} servers and {usersCount} users.");
            await Context.Message.AddReactionAsync(new Emoji("👍"));
        }

        [Command("shutdown")]
        public Task StopHost()
        {
            _ = Host.StopAsync();
            return Task.CompletedTask;
        }


        [Command("eval")]
        public async Task Eval([Remainder] string input)
        {
            var firstBackticks = input.IndexOf('\n', input.IndexOf("```", StringComparison.Ordinal) + 3) + 1;
            var lastBackticks = input.LastIndexOf("```", StringComparison.Ordinal);

            if (firstBackticks == -1 || lastBackticks == -1)
            {
                await ReplyAsync("The code needs to be wrapped in a code block");
                return;
            }

            var msg = await ReplyAsync(embed: new EmbedBuilder()
                .WithTitle("Working...")
                .WithColor(Color.Blue)
                .Build());

            var code = input[firstBackticks..lastBackticks];
            var stopwatch = Stopwatch.StartNew();

            const string usings = "using Discord; " +
                                  "using Discord.Commands; " +
                                  "using System; " +
                                  "using System.Collections.Generic; " +
                                  "using System.Diagnostics; " +
                                  "using System.Linq; " +
                                  "using System.Net.Http; " +
                                  "using System.Reflection; " +
                                  "using System.Text; " +
                                  "using System.Threading.Tasks; " +
                                  "using CobraBot.Handlers; " +
                                  "using CobraBot.Helpers; ";

            code = string.Concat(usings, code);

            //Get assemblies and namespaces
            var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(x => !x.IsDynamic && !string.IsNullOrWhiteSpace(x.Location));

            var namespaces = Assembly.GetEntryAssembly()?.GetTypes()
                .Where(x => !string.IsNullOrEmpty(x.Namespace)).Select(x => x.Namespace).Distinct();

            var options = ScriptOptions.Default
                .AddReferences(assemblies.Select(assembly => MetadataReference.CreateFromFile(assembly.Location)))
                .AddImports(namespaces);

            var script = CSharpScript.Create(code, options, typeof(Globals));

            var diagnostics = script.Compile();

            var compilationTime = stopwatch.ElapsedMilliseconds;

            if (diagnostics.Any(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error))
            {
                var errorBuilder = new EmbedBuilder()
                    .WithTitle("Compilation error")
                    .WithColor(Color.DarkRed)
                    .WithDescription($"**Compile time:** `{compilationTime}ms`\n**Compilation errors:**");

                foreach (var diagnostic in diagnostics)
                {
                    var diagMsg = diagnostic.GetMessage();
                    errorBuilder.AddField(x =>
                    {
                        x.Name = diagnostic.Id;
                        x.Value = diagMsg.Substring(0, Math.Min(500, diagMsg.Length));
                    });
                }

                await msg.ModifyAsync(x => x.Embed = errorBuilder.Build());
                return;
            }

            var context = new Globals { BotContext = BotContext, Context = Context, Host = Host };
            var result = await script.RunAsync(context);
            stopwatch.Stop();

            var resultEmbed = new EmbedBuilder()
                .WithTitle("Result")
                .WithColor(Color.Blue)
                .WithDescription($"{result.ReturnValue.ToString() ?? "_No return_"}")
                .WithFooter($"Took {stopwatch.ElapsedMilliseconds}ms")
                .Build();

            await msg.ModifyAsync(x => x.Embed = resultEmbed);
        }

        public class Globals
        {
            public SocketCommandContext Context { get; set; }
            public BotContext BotContext { get; set; }
            public IHost Host { get; set; }
        }
    }
}