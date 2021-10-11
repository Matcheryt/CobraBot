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
using Discord;
using Discord.Commands;

namespace CobraBot.Modules
{
    [RequireContext(ContextType.Guild)]
    [Name("Utilities")]
    public class UtilitiesModule : ModuleBase<SocketCommandContext>
    {
        //Random number between minVal and maxVal
        [Command("random")]
        [Name("Random")]
        [Summary("Prints random number between two specified numbers.")]
        public async Task RandomNumber([Name("first number")] int minVal = 0, [Name("last number")] int maxVal = 100)
        {
            await ReplyAsync(embed: UtilitiesService.RandomNumber(minVal, maxVal));
        }


        //Poll command
        [Command("poll")]
        [Ratelimit(1, 1, Measure.Seconds)]
        [Name("Poll")]
        [Summary(
            "Creates a poll with specified question and choices. Make sure to write the parameters between \"quotation marks\"")]
        public async Task Poll(string question, [Name("choice 1")] string choice1, [Name("choice 2")] string choice2)
        {
            await UtilitiesService.CreatePollAsync(question, choice1, choice2, Context);
        }


        //Converts specified value from one currency to another
        [Command("convert")]
        [Alias("conversion", "conv")]
        [Ratelimit(1, 1700, Measure.Milliseconds)]
        [Name("Convert")]
        [Summary("Converts value from one currency to another.")]
        public async Task ConvertCurrency(string from, string to, string value)
        {
            await ReplyAsync(embed: await UtilitiesService.ConvertCurrencyAsync(from, to, value));
        }


        //Generate lmgtfy link
        [Command("lmgtfy")]
        [Name("Lmgtfy")]
        [Summary("Creates a lmgtfy link.")]
        public async Task Lmgtfy([Name("text to search")] [Remainder] string textToSearch)
        {
            await ReplyAsync(UtilitiesService.Lmgtfy(textToSearch));
        }


        //Shows user's avatar and provides links for download in various sizes and formats
        [Command("avatar")]
        [Name("Avatar")]
        [Summary(
            "Shows specified users' avatar and provides links for download. If no user is specified, your avatar will be displayed.")]
        public async Task Avatar(IUser user = null)
        {
            await ReplyAsync(embed: UtilitiesService.GetAvatar(Context, user));
        }


        //Shows a color from specified hex color code
        [Command("rgb")]
        [Ratelimit(1, 1, Measure.Seconds)]
        [Name("RGB Color")]
        [Summary("Shows color from specified rgb.")]
        public async Task HexColor(int r, int g, int b)
        {
            await ReplyAsync(embed: await UtilitiesService.GetHexColorAsync(r, g, b));
        }

        //Shows a color from specified hex color code
        [Command("hex")]
        [Ratelimit(1, 1, Measure.Seconds)]
        [Name("Hex Color")]
        [Summary("Shows color from specified hex.")]
        public async Task RgbColor(string hexColor)
        {
            await ReplyAsync(embed: await UtilitiesService.GetRgbColorAsync(hexColor));
        }
    }
}