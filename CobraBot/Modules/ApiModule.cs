using Discord.Commands;
using System.Threading.Tasks;
using CobraBot.Services;

namespace CobraBot.Modules
{
    [RequireContext(ContextType.Guild)]
    [Name("API Module")]
    public class ApiModule : ModuleBase<SocketCommandContext>
    {
        public ApiService ApiService { get; set; }

        //Dictionary Command
        [Command("dict")]
        [Name("Dictionary"), Summary("Retrieves word definition from Oxford Dictionary")]
        public async Task SearchDictionary(string wordToSearch)
            => await ReplyAsync(embed: await ApiService.SearchDictionaryAsync(wordToSearch, Context));


        //API that returns information about a steam user
        [Command("steam")]
        [Name("Steam"), Summary("Shows steam profile from specified user")]
        public async Task GetSteamInfo(string userId)
            => await ReplyAsync(embed: await ApiService.GetSteamInfoAsync(userId));


        //Generate lmgtfy link
        [Command("lmgtfy")]
        [Name("Lmgtfy"), Summary("Creates a lmgtfy link")]
        public async Task Lmgtfy([Remainder] string textToSearch)
            => await ReplyAsync(ApiService.Lmgtfy(textToSearch));


        //Get weather based on user input
        [Command("weather")]
        [Name("Weather"), Summary("Shows weather from specified city")]
        public async Task Weather([Remainder] string city)
            => await ReplyAsync(embed: await ApiService.GetWeatherAsync(city));
    }
}