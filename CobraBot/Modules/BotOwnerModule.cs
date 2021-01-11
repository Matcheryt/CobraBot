using Discord;
using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace CobraBot.Modules
{
    [RequireOwner]
    [Name("Owner")]
    public class BotOwnerModule : ModuleBase<SocketCommandContext>
    {
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
        }
    }
}
