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

using System.Threading.Tasks;
using CobraBot.Preconditions;
using CobraBot.Services;
using Discord.Commands;

namespace CobraBot.Modules
{
    [RequireContext(ContextType.Guild)]
    [Name("Web Services")]
    public class ApiModule : ModuleBase<SocketCommandContext>
    {
        public ApiService ApiService { get; set; }

        //Dictionary Command
        [Command("dict")]
        [Alias("dictionary")]
        [Ratelimit(1, 1350, Measure.Milliseconds)]
        [Name("Dictionary")]
        [Summary("Retrieves word definition from Oxford Dictionary.")]
        public async Task SearchDictionary(string wordToSearch)
        {
            await ReplyAsync(embed: await ApiService.SearchDictionaryAsync(wordToSearch));
        }


        //API that returns information about a steam user
        [Command("steam")]
        [Ratelimit(1, 1350, Measure.Milliseconds)]
        [Name("Steam")]
        [Summary("Shows steam profile from specified user.")]
        public async Task GetSteamInfo([Remainder] string userId)
        {
            await ReplyAsync(embed: await ApiService.GetSteamInfoAsync(userId));
        }


        //Get weather based on user input
        [Command("weather")]
        [Ratelimit(1, 2050, Measure.Milliseconds)]
        [Name("Weather")]
        [Summary("Shows weather from specified city.")]
        public async Task Weather([Remainder] string city)
        {
            await ReplyAsync(embed: await ApiService.GetWeatherAsync(city));
        }


        //Get weather based on user input
        [Command("omdb")]
        [Ratelimit(1, 2050, Measure.Milliseconds)]
        [Name("OMDB")]
        [Summary("Shows movie/series info for specified show. Valid types are `movie`, `series` and `episode`.")]
        public async Task Omdb(string type, [Remainder] string show)
        {
            await ReplyAsync(embed: await ApiService.GetOmdbInformationAsync(type, show));
        }
    }
}