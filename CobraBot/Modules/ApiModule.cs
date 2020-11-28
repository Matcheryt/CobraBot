using Discord.Commands;
using System.Threading.Tasks;
using CobraBot.Services;

namespace CobraBot.Modules
{
    [RequireContext(ContextType.Guild)]
    public class ApiModule : ModuleBase<SocketCommandContext>
    {
        public ApiService ApiService { get; set; }

        //Dictionary Command
        [Command("dict", RunMode = RunMode.Async)]
        public async Task SearchDictionary(string wordToSearch)
            => await ReplyAsync(embed: await ApiService.SearchDictionaryAsync(wordToSearch, Context));


        //API that returns information about a steam user
        [Command("steam", RunMode = RunMode.Async)]
        public async Task GetSteamInfo(string userId)
            => await ReplyAsync(embed: await ApiService.GetSteamInfoAsync(userId));


        //Generate lmgtfy link
        [Command("lmgtfy")]
        public async Task Lmgtfy([Remainder] string textToSearch)
            => await ReplyAsync(ApiService.Lmgtfy(textToSearch));


        //Get weather based on user input
        [Command("weather", RunMode = RunMode.Async)]
        public async Task Weather([Remainder] string city)
            => await ReplyAsync(embed: await ApiService.GetWeatherAsync(city));
    }
}