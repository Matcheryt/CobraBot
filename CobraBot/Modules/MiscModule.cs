using Discord.Commands;
using Discord;
using System.Threading.Tasks;
using System;
using CobraBot.Services;

namespace CobraBot.Modules
{
    [RequireContext(ContextType.Guild)]
    [Name("Misc Module")]
    public class MiscModule : ModuleBase<SocketCommandContext>
    {
        public MiscService MiscService { get; set; }

        //Defines bot's status
        [RequireOwner]
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

        //Shows discord user info
        [Command("usinfo"), Alias("whois", "user")]
        [Name("User info"), Summary("Shows discord info for specified user")]
        public async Task GetInfo(IGuildUser user = null)
            => await ReplyAsync(embed: MiscService.ShowUserInfoAsync(user));

        //Converts specified value from one currency to another
        [Command("convert"), Alias("conversion", "conv")]
        [Name("Convert"), Summary("Converts value from one currency to another")]
        public async Task ConvertCurrency(string from, string to, string value)
            => await ReplyAsync(embed: await MiscService.ConvertCurrencyAsync(from, to, value));
    }
}