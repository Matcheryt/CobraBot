using CobraBot.Services;
using Discord.Commands;
using System.Threading.Tasks;

namespace CobraBot.Modules
{
    [RequireContext(ContextType.Guild)]
    [Name("Web Services")]
    public class ApiModule : ModuleBase<SocketCommandContext>
    {
        public ApiService ApiService { get; set; }

        //Dictionary Command
        [Command("dict"), Alias("dictionary")]
        [Name("Dictionary"), Summary("Retrieves word definition from Oxford Dictionary.")]
        public async Task SearchDictionary(string wordToSearch)
            => await ReplyAsync(embed: await ApiService.SearchDictionaryAsync(wordToSearch));


        //API that returns information about a steam user
        [Command("steam")]
        [Name("Steam"), Summary("Shows steam profile from specified user.")]
        public async Task GetSteamInfo([Remainder] string userId)
            => await ReplyAsync(embed: await ApiService.GetSteamInfoAsync(userId));


        //Get weather based on user input
        [Command("weather")]
        [Name("Weather"), Summary("Shows weather from specified city.")]
        public async Task Weather([Remainder] string city)
            => await ReplyAsync(embed: await ApiService.GetWeatherAsync(city));


        //Get weather based on user input
        [Command("omdb")]
        [Name("OMDB"), Summary("Shows movie/series info for specified show. Valid types are `movie`, `series` and `episode`.")]
        public async Task Omdb(string type, [Remainder] string show)
            => await ReplyAsync(embed: await ApiService.GetOmdbInformationAsync(type, show));
    }
}