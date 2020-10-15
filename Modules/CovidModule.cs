using System;
using System.Net;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Newtonsoft.Json.Linq;

namespace CobraBot.Modules
{
    public class CovidModule : ModuleBase<SocketCommandContext>
    {
        Helpers.Helpers helper = new Helpers.Helpers();

        [Command("covid", RunMode = RunMode.Async)]
        public async Task Covid([Remainder] string area = "")
        {
            string jsonResponse;
            JObject jsonParsed;

            try
            {
                if (area.ToLower() == "portugal")
                {
                    jsonResponse = await helper.HttpRequestAndReturnJson("https://covid19-api.vost.pt/Requests/get_last_update");

                    jsonParsed = JObject.Parse(jsonResponse);
                    string confirmadosNovos = (string)jsonParsed["confirmados_novos"];
                    string casosConfirmados = (string)jsonParsed["confirmados"];
                    string data = (string)jsonParsed["data"];
                    string mortes = (string)jsonParsed["obitos"];
                    string recuperados = (string)jsonParsed["recuperados"];

                    EmbedBuilder builder = new EmbedBuilder()
                    .WithTitle("Portugal COVID19 data")
                    .WithDescription($"New cases: {confirmadosNovos}\nConfirmed cases: {casosConfirmados}\nDeaths: {mortes}\nRecovered: {recuperados}")
                    .WithFooter($"Last updated: {data}")
                    .WithColor(Color.DarkBlue);

                    await ReplyAsync("", false, builder.Build());
                }
                else if (area == "")
                {
                    jsonResponse = await helper.HttpRequestAndReturnJson("https://api.covid19api.com/world/total");

                    jsonParsed = JObject.Parse(jsonResponse);

                    string totalConfirmed = (string)jsonParsed["TotalConfirmed"];
                    string totalDeaths = (string)jsonParsed["TotalDeaths"];
                    string totalRecovered = (string)jsonParsed["TotalRecovered"];

                    EmbedBuilder builder = new EmbedBuilder()
                    .WithTitle("Live world COVID19 data")
                    .WithDescription($"Total confirmed: {totalConfirmed}\nTotal deaths: {totalDeaths}\nTotal recovered: {totalRecovered}")
                    .WithColor(Color.DarkBlue);

                    await ReplyAsync("", false, builder.Build());
                }
                else
                {
                    jsonResponse = await helper.HttpRequestAndReturnJson("https://api.covid19api.com/total/dayone/country/" + area);

                    if (jsonResponse.Contains("Not found"))
                    {
                        helper.errorBuilder.WithDescription("Country requested not found!");
                        await ReplyAsync("", false, helper.errorBuilder.Build());
                    }

                    var jsonParsedArray = JArray.Parse(jsonResponse);

                    string confirmed = (string)jsonParsedArray.Last["Confirmed"];
                    string deaths = (string)jsonParsedArray.Last["Deaths"];
                    string recovered = (string)jsonParsedArray.Last["Recovered"];
                    string active = (string)jsonParsedArray.Last["Active"];
                    string date = (string)jsonParsedArray.Last["Date"];
                    string country = (string)jsonParsedArray.Last["Country"];        

                    EmbedBuilder builder = new EmbedBuilder()
                    .WithTitle(country + " COVID19 data")
                    .WithDescription($"Confirmed: {confirmed}\nDeaths: {deaths}\nRecovered: {recovered}\nActive: {active}")
                    .WithFooter($"Last updated: {date}")
                    .WithColor(Color.DarkBlue);

                    await ReplyAsync("", false, builder.Build());
                }
            }
            catch(Exception e)
            {
                
            }      
        }       
    }
}