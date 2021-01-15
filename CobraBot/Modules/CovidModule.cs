using CobraBot.Common.EmbedFormats;
using CobraBot.Handlers;
using CobraBot.Helpers;
using Discord;
using Discord.Commands;
using Newtonsoft.Json.Linq;
using System;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CobraBot.Common;

namespace CobraBot.Modules
{
    [Name("Covid")]
    public class CovidModule : ModuleBase<SocketCommandContext>
    {
        //COVID19 command
        [Command("covid")]
        [Name("Covid"), Summary("Displays covid info for specified country.")]
        public async Task Covid([Remainder] string countryToSearch = "")
        {
            try
            {
                Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

                //Different api for Portugal since I'm portuguese and there's a dedicated api just for portuguese covid data
                //If user searches for portugal
                JObject jsonParsed;

                if (countryToSearch.ToLower() == "portugal" || countryToSearch.ToLower() == "pt" || countryToSearch.ToLower() == "prt")
                {
                    //Request portugal covid data from api
                    var request = new HttpRequestMessage()
                    {
                        RequestUri = new Uri("https://covid19-api.vost.pt/Requests/get_last_update"),
                        Method = HttpMethod.Get
                    };

                    jsonParsed = JObject.Parse(await Helper.HttpRequestAndReturnJson(request));

                    int confirmadosNovos = (int)jsonParsed["confirmados_novos"];
                    int casosConfirmados = (int)jsonParsed["confirmados"];
                    string data = (string)jsonParsed["data"];
                    int mortes = (int)jsonParsed["obitos"];
                    int recuperados = (int)jsonParsed["recuperados"];

                    var builder = new EmbedBuilder()
                    .WithTitle($"Portugal COVID19 data {CustomEmotes.CovidEmote}")
                    .WithDescription($"New cases: {confirmadosNovos:n0}\nConfirmed cases: {casosConfirmados:n0}\nDeaths: {mortes:n0}\nRecovered: {recuperados:n0}")
                    .WithFooter($"Last updated: {data}")
                    .WithColor(Color.DarkBlue);

                    await ReplyAsync("", false, builder.Build());
                }
                //If countryToSearch isn't specified, then show world covid data
                else if (countryToSearch == "")
                {
                    //Request world covid data from api
                    var request = new HttpRequestMessage()
                    {
                        RequestUri = new Uri("https://api.covid19api.com/world/total"),
                        Method = HttpMethod.Get
                    };

                    jsonParsed = JObject.Parse(await Helper.HttpRequestAndReturnJson(request));

                    int totalConfirmed = (int)jsonParsed["TotalConfirmed"];
                    int totalDeaths = (int)jsonParsed["TotalDeaths"];
                    int totalRecovered = (int)jsonParsed["TotalRecovered"];

                    var builder = new EmbedBuilder()
                    .WithTitle($"Live world COVID19 data {CustomEmotes.CovidEmote}")
                    .WithDescription($"Total confirmed: {totalConfirmed:n0}\nTotal deaths: {totalDeaths:n0}\nTotal recovered: {totalRecovered:n0}")
                    .WithColor(Color.DarkBlue);

                    await ReplyAsync("", false, builder.Build());
                }
                //If the countryToSearch != to portugal but if the countryToSearch is specified
                else
                {
                    //Request specified countryToSearch covid data from api
                    var request = new HttpRequestMessage()
                    {
                        RequestUri = new Uri("https://api.covid19api.com/total/dayone/country/" + countryToSearch),
                        Method = HttpMethod.Get
                    };

                    var jsonParsedArray = JArray.Parse(await Helper.HttpRequestAndReturnJson(request));

                    /* We use jsonParsedArray.Last here because the json response returns the list of
                       all cases since Day One, and by using jsonParsedArray.Last we know that the value
                       is going to be the most recent one.*/
                    int confirmed = (int)jsonParsedArray.Last["Confirmed"];
                    int deaths = (int)jsonParsedArray.Last["Deaths"];
                    int recovered = (int)jsonParsedArray.Last["Recovered"];
                    int active = (int)jsonParsedArray.Last["Active"];
                    string date = (string)jsonParsedArray.Last["Date"];
                    string country = (string)jsonParsedArray.Last["Country"];

                    var builder = new EmbedBuilder()
                    .WithTitle($"{country} COVID19 data {CustomEmotes.CovidEmote}")
                    .WithDescription($"Confirmed: {confirmed:n0}\nDeaths: {deaths:n0}\nRecovered: {recovered:n0}\nActive: {active:n0}")
                    .WithFooter($"Last updated: {date}")
                    .WithColor(Color.DarkBlue);

                    await ReplyAsync("", false, builder.Build());
                }
            }
            catch (Exception e)
            {
                var httpException = (HttpRequestException)e;

                //Error handling
                switch (httpException.StatusCode)
                {
                    //If not found
                    case HttpStatusCode.NotFound:
                        await ReplyAsync(embed: CustomFormats.CreateErrorEmbed("**Country not found!** Please try again."));
                        break;

                    //If bad request
                    case HttpStatusCode.BadRequest:
                        await ReplyAsync(embed: CustomFormats.CreateErrorEmbed("**Not supported!**"));
                        break;
                }

                await ReplyAsync(embed: CustomFormats.CreateErrorEmbed($"An error occurred\n{e.Message}"));
            }
        }
    }
}