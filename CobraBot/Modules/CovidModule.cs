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
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using CobraBot.Common;
using CobraBot.Common.EmbedFormats;
using CobraBot.Helpers;
using CobraBot.Preconditions;
using Discord;
using Discord.Commands;
using Newtonsoft.Json.Linq;

namespace CobraBot.Modules
{
    [Name("Covid")]
    public class CovidModule : ModuleBase<SocketCommandContext>
    {
        //COVID19 command
        [Command("covid")]
        [Ratelimit(1, 2, Measure.Seconds)]
        [Name("Covid")]
        [Summary("Displays covid info for specified country.")]
        public async Task Covid([Remainder] string countryToSearch = "")
        {
            try
            {
                Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

                //Different api for Portugal since I'm portuguese and there's a dedicated api just for portuguese covid data
                //If user searches for portugal
                JObject jsonParsed;

                if (countryToSearch.ToLower() == "portugal" || countryToSearch.ToLower() == "pt" ||
                    countryToSearch.ToLower() == "prt")
                {
                    //Request portugal covid data from api
                    var request =
                        await HttpHelper.HttpClient.GetAsync("https://covid19-api.vost.pt/Requests/get_last_update");

                    jsonParsed = JObject.Parse(await request.Content.ReadAsStringAsync());

                    var confirmadosNovos = (int)jsonParsed["confirmados_novos"];
                    var casosConfirmados = (int)jsonParsed["confirmados"];
                    var data = (string)jsonParsed["data"];
                    var mortes = (int)jsonParsed["obitos"];
                    var recuperados = (int)jsonParsed["recuperados"];

                    var builder = new EmbedBuilder()
                        .WithTitle($"Portugal COVID19 data {CustomEmotes.CovidEmote}")
                        .WithDescription(
                            $"New cases: {confirmadosNovos:n0}\nConfirmed cases: {casosConfirmados:n0}\nDeaths: {mortes:n0}\nRecovered: {recuperados:n0}")
                        .WithFooter($"Last update: {data}")
                        .WithColor(Color.DarkBlue);

                    await ReplyAsync("", false, builder.Build());
                }
                //If countryToSearch isn't specified, then show world covid data
                else if (countryToSearch == "")
                {
                    //Request world covid data from api
                    var request = await HttpHelper.HttpClient.GetAsync("https://corona-api.com/timeline");

                    jsonParsed = JObject.Parse(await request.Content.ReadAsStringAsync());

                    var totalConfirmed = (int)jsonParsed["data"].First["confirmed"];
                    var totalDeaths = (int)jsonParsed["data"].First["deaths"];
                    var totalRecovered = (int)jsonParsed["data"].First["recovered"];
                    _ = DateTime.TryParse(jsonParsed["data"].First["updated_at"].ToString(), out var updatedAt);

                    var builder = new EmbedBuilder()
                        .WithTitle($"Live world COVID19 data {CustomEmotes.CovidEmote}")
                        .WithDescription(
                            $"Total confirmed: {totalConfirmed:n0}\nTotal deaths: {totalDeaths:n0}\nTotal recovered: {totalRecovered:n0}")
                        .WithFooter($"Last update: {updatedAt:dd/MM/yyyy HH:mm:ss}")
                        .WithColor(Color.DarkBlue);

                    await ReplyAsync("", false, builder.Build());
                }
                //If the countryToSearch != to portugal but if the countryToSearch is specified
                else
                {
                    var request =
                        await HttpHelper.HttpClient.GetAsync("https://api.covid19api.com/total/dayone/country/" +
                                                             countryToSearch);

                    var jsonParsedArray = JArray.Parse(await request.Content.ReadAsStringAsync());

                    /* We use jsonParsedArray.Last here because the json response returns the list of
                       all cases since Day One, and by using jsonParsedArray.Last we know that the value
                       is going to be the most recent one.*/
                    var confirmed = (int)jsonParsedArray.Last["Confirmed"];
                    var deaths = (int)jsonParsedArray.Last["Deaths"];
                    var recovered = (int)jsonParsedArray.Last["Recovered"];
                    var active = (int)jsonParsedArray.Last["Active"];
                    _ = DateTime.TryParse(jsonParsedArray.Last["Date"].ToString(), out var updatedAt);
                    var country = (string)jsonParsedArray.Last["Country"];

                    var builder = new EmbedBuilder()
                        .WithTitle($"{country} COVID19 data {CustomEmotes.CovidEmote}")
                        .WithDescription(
                            $"Confirmed: {confirmed:n0}\nDeaths: {deaths:n0}\nRecovered: {recovered:n0}\nActive: {active:n0}")
                        .WithFooter($"Last update: {updatedAt:dd/MM/yyyy}")
                        .WithColor(Color.DarkBlue);

                    await ReplyAsync("", false, builder.Build());
                }
            }
            catch (Exception)
            {
                await ReplyAsync(embed: CustomFormats.CreateErrorEmbed("Country not found!"));
            }
        }
    }
}