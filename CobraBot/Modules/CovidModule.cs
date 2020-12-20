﻿using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using CobraBot.Common;
using CobraBot.Handlers;
using CobraBot.Helpers;
using Discord;
using Discord.Commands;
using Newtonsoft.Json.Linq;

namespace CobraBot.Modules
{
    [Name("Covid Module")]
    public class CovidModule : ModuleBase<SocketCommandContext>
    {
        //COVID19 command
        [Command("covid")]
        [Name("Covid"), Summary("Displays covid info for specified country")]
        public async Task Covid([Remainder] string countryToSearch = "")
        {
            try
            {
                //Different api for Portugal since I'm portuguese and there's a dedicated api just for portuguese covid data
                //If user searches for portugal
                JObject jsonParsed;

                if (countryToSearch.ToLower() == "portugal" || countryToSearch.ToLower() == "pt" || countryToSearch.ToLower() == "prt")
                {
                    //Request portugal covid data from api
                    var request = new HttpRequestMessage()
                    {
                        RequestUri = new Uri("https://covid19-api.vost.pt/Requests/get_last_update"),
                        Method = HttpMethod.Get,
                        Headers =
                        {
                            { "Authorization", $"Bearer {Configuration.KSoftApiKey}" }
                        }
                    };

                    jsonParsed = JObject.Parse(await Helper.HttpRequestAndReturnJson(request));

                    string confirmadosNovos = (string)jsonParsed["confirmados_novos"];
                    string casosConfirmados = (string)jsonParsed["confirmados"];
                    string data = (string)jsonParsed["data"];
                    string mortes = (string)jsonParsed["obitos"];
                    string recuperados = (string)jsonParsed["recuperados"];

                    var builder = new EmbedBuilder()
                    .WithTitle("Portugal COVID19 data")
                    .WithDescription($"New cases: {confirmadosNovos}\nConfirmed cases: {casosConfirmados}\nDeaths: {mortes}\nRecovered: {recuperados}")
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
                        Method = HttpMethod.Get,
                        Headers =
                        {
                            { "Authorization", $"Bearer {Configuration.KSoftApiKey}" }
                        }
                    };

                    jsonParsed = JObject.Parse(await Helper.HttpRequestAndReturnJson(request));

                    int totalConfirmed = (int)jsonParsed["TotalConfirmed"];
                    int totalDeaths = (int)jsonParsed["TotalDeaths"];
                    int totalRecovered = (int)jsonParsed["TotalRecovered"];

                    var builder = new EmbedBuilder()
                    .WithTitle("Live world COVID19 data")
                    .WithDescription($"Total confirmed: {totalConfirmed}\nTotal deaths: {totalDeaths}\nTotal recovered: {totalRecovered}")
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
                        Method = HttpMethod.Get,
                        Headers =
                        {
                            { "Authorization", $"Bearer {Configuration.KSoftApiKey}" }
                        }
                    };

                    var jsonParsedArray = JArray.Parse(await Helper.HttpRequestAndReturnJson(request));

                    /* We use jsonParsedArray.Last here because the json response returns the list of
                       all cases since Day One, and by using jsonParsedArray.Last we know that the value
                       is going to be the most recent one.*/
                    string confirmed = (string)jsonParsedArray.Last["Confirmed"];
                    string deaths = (string)jsonParsedArray.Last["Deaths"];
                    string recovered = (string)jsonParsedArray.Last["Recovered"];
                    string active = (string)jsonParsedArray.Last["Active"];
                    string date = (string)jsonParsedArray.Last["Date"];
                    string country = (string)jsonParsedArray.Last["Country"];
                    
                    var builder = new EmbedBuilder()
                    .WithTitle(country + " COVID19 data")
                    .WithDescription($"Confirmed: {confirmed}\nDeaths: {deaths}\nRecovered: {recovered}\nActive: {active}")
                    .WithFooter($"Last updated: {date}")
                    .WithColor(Color.DarkBlue);

                    await ReplyAsync("", false, builder.Build());
                }
            }
            catch(Exception e)
            {
                var httpException = (HttpRequestException)e;

                //Error handling
                switch (httpException.StatusCode)
                {
                    //If not found
                    case HttpStatusCode.NotFound:
                        await ReplyAsync(embed: EmbedFormats.CreateErrorEmbed("**Country not found!** Please try again."));
                        break;
                    
                    //If bad request
                    case HttpStatusCode.BadRequest:
                        await ReplyAsync(embed: EmbedFormats.CreateErrorEmbed("**Not supported!**"));
                        break;
                }

                await ReplyAsync(embed: EmbedFormats.CreateErrorEmbed($"An error occurred\n{e.Message}"));
            }      
        }       
    }
}