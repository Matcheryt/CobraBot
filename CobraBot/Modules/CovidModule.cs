using System.Net;
using System.Threading.Tasks;
using CobraBot.Helpers;
using Discord;
using Discord.Commands;
using Newtonsoft.Json.Linq;

namespace CobraBot.Modules
{
    public class CovidModule : ModuleBase<SocketCommandContext>
    {
        //COVID19 command
        [Command("covid", RunMode = RunMode.Async)]
        public async Task Covid([Remainder] string area = "")
        {
            string jsonResponse;
            JObject jsonParsed;

            try
            {
                //Different api for Portugal since I'm portuguese and there's a dedicated api just for portuguese covid data
                //If user searches for portugal
                if (area.ToLower() == "portugal" || area.ToLower() == "pt" || area.ToLower() == "prt")
                {
                    //Request portugal covid data from api
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://covid19-api.vost.pt/Requests/get_last_update");

                    jsonResponse = await Helper.HttpRequestAndReturnJson(request);

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
                //If area isn't specified, then show world covid data
                else if (area == "")
                {
                    //Request world covid data from api
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://api.covid19api.com/world/total");

                    jsonResponse = await Helper.HttpRequestAndReturnJson(request);

                    jsonParsed = JObject.Parse(jsonResponse);

                    int totalConfirmed = (int)jsonParsed["TotalConfirmed"];
                    int totalDeaths = (int)jsonParsed["TotalDeaths"];
                    int totalRecovered = (int)jsonParsed["TotalRecovered"];

                    EmbedBuilder builder = new EmbedBuilder()
                    .WithTitle("Live world COVID19 data")
                    .WithDescription($"Total confirmed: {totalConfirmed}\nTotal deaths: {totalDeaths}\nTotal recovered: {totalRecovered}")
                    .WithColor(Color.DarkBlue);

                    await ReplyAsync("", false, builder.Build());
                }
                //If the area != to portugal but if the area is specified
                else
                {
                    //Request specified area covid data from api
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://api.covid19api.com/total/dayone/country/" + area);

                    jsonResponse = await Helper.HttpRequestAndReturnJson(request);

                    var jsonParsedArray = JArray.Parse(jsonResponse);

                    /* We use jsonParsedArray.Last here because the json response returns the list of
                       all cases since Day One, and by using jsonParsedArray.Last we know that the value
                       is going to be the most recent one.*/
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
            catch(WebException e)
            {
                if (e.Status == WebExceptionStatus.ProtocolError)
                {
                    //Error handling
                    if (((HttpWebResponse)e.Response).StatusCode == HttpStatusCode.NotFound)
                    {
                        await ReplyAsync(embed: await Helper.CreateErrorEmbed("**Country not found!** Please try again."));
                    }
                    if (((HttpWebResponse)e.Response).StatusCode == HttpStatusCode.BadRequest)
                    {
                        await ReplyAsync(embed: await Helper.CreateErrorEmbed("**Not supported!**"));
                    }
                }
            }      
        }       
    }
}