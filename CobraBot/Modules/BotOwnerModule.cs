using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CobraBot.Handlers;
using SpotifyAPI.Web;

namespace CobraBot.Modules
{
    [RequireOwner]
    [Name("Owner")]
    public class BotOwnerModule : ModuleBase<SocketCommandContext>
    {
        public SpotifyClient SpotifyClient { get; set; }

        //[Command("teste")]
        //public async Task Teste(string url)
        //{
        //    var items = await SpotifyClient.Playlists.GetItems(url);

        //    foreach (var item in items.Items)
        //    {
        //        if (item.Track is FullTrack track)
        //            Console.WriteLine(track.Name);
        //    }
        //}

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
            Console.WriteLine($"{DateTime.Now}: Cobra's status was changed to {status} with activity type: {activityType}");
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

        //Update discord bot lists
        [Command("updatebotlists")]
        public async Task UpdateBotLists()
        {
            var serverCount = Context.Client.Guilds.Count;
            var usersCount = Context.Client.Guilds.Sum(x => x.MemberCount);

            var dblApiUrl = $"https://discordbotlist.com/api/v1/bots/{Context.Client.CurrentUser.Id}/stats";
            var topApiUrl = $"https://top.gg/api/bots/{Context.Client.CurrentUser.Id}/stats";

            var httpClient = new HttpClient();

            //Discord bot list
            var dblContent = new Dictionary<string, string>()
            {
                { "users", $"{usersCount}"},
                { "guilds", $"{serverCount}" }
            };

            //Top.gg bot list
            var topContent = new StringContent($"{{\"server_count\":\"{serverCount}\"}}", Encoding.UTF8, "application/json");

            //Send requests
            httpClient.DefaultRequestHeaders.Add("Authorization", Configuration.DblApiKey);
            await httpClient.PostAsync(dblApiUrl, new FormUrlEncodedContent(dblContent));

            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("Authorization", Configuration.TopggApiKey);
            await httpClient.PostAsync(topApiUrl, topContent);

            httpClient.Dispose();

            await Context.Message.AddReactionAsync(new Emoji("👍"));
        }
    }
}
