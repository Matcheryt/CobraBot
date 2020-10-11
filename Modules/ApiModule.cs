using Discord.Commands;
using Discord;
using System.Threading.Tasks;
using System;
using System.Net;
using Newtonsoft.Json.Linq;
using System.IO;

namespace CobraBot.Modules
{
    public class ApiModule : ModuleBase<SocketCommandContext>
    {
        
        /* API Documentation
         * Steam: https://developer.valvesoftware.com/wiki/Steam_Web_API
         * OpenWeatherMap: https://openweathermap.org/api
         * Oxford Dictionary: https://developer.oxforddictionaries.com/documentation */

        //Default embed to show an error
        EmbedBuilder errorBuilder = new EmbedBuilder().WithColor(Color.Red);
        
        CobraBot.Helpers.Helpers helper = new Helpers.Helpers();

        #region ApiKeys
        string dictApiKey;
        string dictAppId;
        string steamDevKey;
        string owmApiKey;

        private Configuration config = new Configuration();

        //Constructor initializing API key strings from config file
        private ApiModule()
        {
            dictApiKey = config.ReturnSavedValue("APIKEYS", "OxfordDictionary");
            dictAppId = config.ReturnSavedValue("APIKEYS", "OxfordAppId");
            steamDevKey = config.ReturnSavedValue("APIKEYS", "Steam");
            owmApiKey = config.ReturnSavedValue("APIKEYS", "OWM");
        }
        #endregion


        //Dictionary Command
        [Command("dict")]
        public async Task SearchDictionary(string wordToSearch)
        {            
            string json = string.Empty;          

            try
            {
                //Make request with necessary headers
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://od-api.oxforddictionaries.com/api/v2/entries/en-gb/" + wordToSearch.ToLower() + "?strictMatch=false");
                request.Method = "GET";
                request.ContinueTimeout = 12000;
                request.Accept = "application/json";
                request.Headers["app_id"] = dictAppId;
                request.Headers["app_key"] = dictApiKey;

                using (HttpWebResponse response = (HttpWebResponse)(await request.GetResponseAsync()))
                {
                    //Process the response
                    using (Stream stream = response.GetResponseStream())
                    using (StreamReader reader = new StreamReader(stream))
                        json += await reader.ReadToEndAsync();

                    //And parse the json
                    JObject jsonParsed = JObject.Parse(json);

                    var wordDefinition = jsonParsed["results"][0]["lexicalEntries"][0]["entries"][0]["senses"][0]["definitions"][0];
                    var wordExample = jsonParsed["results"][0]["lexicalEntries"][0]["entries"][0]["senses"][0]["examples"][0]["text"];
                    var synonyms = jsonParsed["results"][0]["lexicalEntries"][1]["entries"][0]["senses"][0]["synonyms"][0]["text"];

                    var embed = new EmbedBuilder();
                    embed.WithTitle(wordToSearch.ToUpper() + " Meaning")
                        .WithDescription("**Definition:\n  **" + wordDefinition + "\n**Example:\n  **" + wordExample + "\n**Synonyms**\n  " + synonyms)
                        .WithColor(Color.DarkMagenta);

                    await ReplyAsync("", false, embed.Build());
                }
            }
            catch (WebException e)
            {
                if (e.Status == WebExceptionStatus.ProtocolError)
                {
                    HttpStatusCode code = ((HttpWebResponse)e.Response).StatusCode;
                    if (code == HttpStatusCode.NotFound)
                        await ReplyAsync("Word requested not found.");
                    if (code == HttpStatusCode.BadRequest)
                        await ReplyAsync("Word not supported.");
                }
            }
        }

        #region SteamAPI

        //API that returns information about a steam user
        [Command("steam", RunMode = RunMode.Async)]
        public async Task GetSteamInfo(string userId)
        {
            string id64response = null;

            //Variables
            string steamName = null;
            string realName = null;
            ulong steam64ID = 0;
            string avatarUrl = null;
            int onlineStatusGet = 0;
            string profileUrl = null;
            string countryCode = null;            
            int profileVisibilityGet = 0;
            int steamUserLevel = 0;

            //Not the best way to verify if user is inputing the vanityURL or the SteamID, but it works
            //Verify if steam ID contains only numbers
            if (!helper.IsDigitsOnly(userId))
            {
                //If not, get steam id 64 based on user input and get steam level also
                id64response = await GetSteamId64(userId);
                steamUserLevel = await GetSteamLevel(id64response);
            }
            else
            {
                //If it is digits only, then assume the user input is the steam 64 id of a steam profile
                id64response = userId;
                steamUserLevel = await GetSteamLevel(id64response);
            }

            try
            {
                //Create web request, requesting player profile info
                string httpResponse = null;
                httpResponse = await helper.HttpRequestAndReturnJson("https://api.steampowered.com/ISteamUser/GetPlayerSummaries/v2/?key=" + steamDevKey + "&steamids=" + id64response);

                //Parse the json from httpResponse
                JObject profileJsonResponse = JObject.Parse(httpResponse);

                //Give values to the variables
                try
                {
                    steamName = (string)profileJsonResponse["response"]["players"][0]["personaname"];
                    realName = (string)profileJsonResponse["response"]["players"][0]["realname"];
                    steam64ID = (ulong)profileJsonResponse["response"]["players"][0]["steamid"];
                    avatarUrl = (string)profileJsonResponse["response"]["players"][0]["avatarfull"];
                    onlineStatusGet = (int)profileJsonResponse["response"]["players"][0]["personastate"];
                    profileUrl = (string)profileJsonResponse["response"]["players"][0]["profileurl"];
                    countryCode = (string)profileJsonResponse["response"]["players"][0]["loccountrycode"];
                    profileVisibilityGet = (int)profileJsonResponse["response"]["players"][0]["communityvisibilitystate"];
                }
                catch (Exception)
                {
                    errorBuilder.WithDescription("**User not found!** Please check your SteamID and try again.");
                    await ReplyAsync("", false, errorBuilder.Build());
                    return;
                }

                //Online Status Switch
                string onlineStatus = null;
                switch (onlineStatusGet)
                {
                    case 0:
                        onlineStatus = "Offline";
                        break;
                    case 1:
                        onlineStatus = "Online";
                        break;
                    case 2:
                        onlineStatus = "Busy";
                        break;
                    case 3:
                        onlineStatus = "Away";
                        break;
                    case 4:
                        onlineStatus = "Snooze";
                        break;
                    case 5:
                        onlineStatus = "Looking to Trade";
                        break;
                    case 6:
                        onlineStatus = "Looking to Play";
                        break;

                }

                //Profile Visibility Switch
                string profileVisibility = null;
                switch (profileVisibilityGet)
                {
                    case 1:
                        profileVisibility = "Private";
                        break;
                    case 2:
                        profileVisibility = "Friends Only";
                        break;
                    case 3:
                        profileVisibility = "Public";
                        break;
                }

                var embed = new EmbedBuilder();
                embed.WithTitle(steamName + " Steam Info")
                    .WithDescription("\nSteam Name: " + "**" + steamName + "**" + "\nSteam Level: " + "**" + steamUserLevel + "**" + "\nReal Name: " + "**" + realName + "**" + "\nSteam ID64: " + "**" + steam64ID + "**" + "\nStatus: " + "**" + onlineStatus + "**" + "\nProfile Privacy: " + "**" + profileVisibility + "**" + "\nCountry: " + "**" + countryCode + "**" + "\n\nProfile URL: " + profileUrl)
                    .WithThumbnailUrl(avatarUrl)
                    .WithColor(Color.Blue);

                await ReplyAsync("", false, embed.Build());
            }
            catch (WebException)
            {
                errorBuilder.WithDescription("**An error ocurred**");
                await ReplyAsync("", false, errorBuilder.Build());
            }

        }


        /* If user input on "steam" command isn't the steamId64 but instead the vanity url
        create a request that will return the steamId64 based on the vanity url*/
        public async Task<string> GetSteamId64(string userId)
        {
            TaskCompletionSource<String> tcs = new TaskCompletionSource<String>();

            string userIdResolved;

            //Create request
            string httpResponse = null;
            httpResponse = await helper.HttpRequestAndReturnJson("http://api.steampowered.com/ISteamUser/ResolveVanityURL/v0001/?key=" + steamDevKey + "&vanityurl=" + userId);

            //Save steamResponse in a string and then retrieve user steamId64
            try
            {
                JObject jsonParsed = JObject.Parse(httpResponse);

                userIdResolved = jsonParsed["response"]["steamid"].ToString();

                tcs.SetResult(userIdResolved);
            }
            catch (NullReferenceException)
            {
                errorBuilder.WithDescription("**User not found!** Please check your SteamID and try again.");
                await ReplyAsync("", false, errorBuilder.Build());
            }

            string result = await tcs.Task;

            return result;
        }


        //Get steam level
        public async Task<int>GetSteamLevel(string userId)
        {
            TaskCompletionSource<int> tcs = new TaskCompletionSource<int>();

            int userLevel = 0;

            //Create a webRequest to steam api endpoint
            string httpResponse = null;
            httpResponse = await helper.HttpRequestAndReturnJson("http://api.steampowered.com/IPlayerService/GetSteamLevel/v1/?key=" + steamDevKey + "&steamid=" + userId);
            
            //Save steamResponse in a string and then retrieve user level
            try
            {
                //Parse the json from httpResponse
                JObject jsonParsed = JObject.Parse(httpResponse);

                userLevel = (int)jsonParsed["response"]["player_level"];

                tcs.SetResult(userLevel);
            }
            catch (NullReferenceException)
            {
                errorBuilder.WithDescription("**Couldn't fetch level**");
                await ReplyAsync("", false, errorBuilder.Build());
            }

            int result = await tcs.Task;

            return result;
        }

        #endregion

        //Generate lmgtfy link
        [Command("lmgtfy")]
        public async Task Lmgtfy(params string[] textToSearch)
        {
            if (textToSearch == null || textToSearch.Length == 0)
            {
                errorBuilder.WithDescription("**Please specify some text**");
                await ReplyAsync("", false, errorBuilder.Build());
                return;
            }

            try
            {
                string textSearchConverted = String.Join(" ", textToSearch);

                if (textSearchConverted.Contains(" "))
                {
                    string textSearchWithoutWhiteSpace = textSearchConverted.Replace(" ", "+");
                    await ReplyAsync("http://lmgtfy.com/?q=" + textSearchWithoutWhiteSpace);
                }
                else
                {
                    await ReplyAsync("http://lmgtfy.com/?q=" + textSearchConverted);
                }
            }
            catch (Exception)
            {
                await ReplyAsync("Why the fuck did lmgtfy command throw an error?");
            }
            
        }


        //Get weather based on user input
        [Command("weather")]
        public async Task Weather(params string[] city)
        {
            //If no arguments passed, reply with a error
            if (city.Length == 0 || city == null)
            {
                errorBuilder.WithDescription("**Please specify a location**");
                await ReplyAsync("", false, errorBuilder.Build());
                return;
            }

            //Joins two words into a string
            string cityConverted = String.Join(" ", city);
            string searchQuery = null;

            //Replaces white spaces with + signal
            if(cityConverted.Contains(" "))
            {
                searchQuery = cityConverted.Replace(" ", "+");
            }
            else
            {
                searchQuery = cityConverted;
            }           

            try
            {
                //Request weather from OWM and return json
                string httpResponse = null;
                httpResponse = await helper.HttpRequestAndReturnJson("http://api.openweathermap.org/data/2.5/weather?q=" + searchQuery + "&appid=" + owmApiKey + "&units=metric");

                //Parse the json from httpResponse
                JObject weatherParsedJson = JObject.Parse(httpResponse);

                //Give values to the variables
                string weatherMain = (string)weatherParsedJson["weather"][0]["main"];
                string weatherDescription = (string)weatherParsedJson["weather"][0]["description"];
                string thumbnailIcon = (string)weatherParsedJson["weather"][0]["icon"];
                string cityName = (string)weatherParsedJson["name"];
                string cityCountry = (string)weatherParsedJson["sys"]["country"];
                double actualTemperature = (double)weatherParsedJson["main"]["temp"];
                double maxTemperature = (double)weatherParsedJson["main"]["temp_max"];
                double minTemperature = (double)weatherParsedJson["main"]["temp_min"];
                double humidity = (double)weatherParsedJson["main"]["humidity"];

                weatherDescription = helper.FirstLetterToUpper(weatherDescription);

                string thumbnailUrl = "http://openweathermap.org/img/w/" + thumbnailIcon + ".png";

                //Send message with the current weather
                var embed = new EmbedBuilder();
                embed.WithTitle("Current Weather for: " + cityName + ", " + cityCountry)
                    .WithThumbnailUrl(thumbnailUrl)
                    .WithDescription("**Conditions: " + weatherMain + ", " + weatherDescription + "**\nTemperature: **" + actualTemperature + "ºC**\n" + "Max Temperature: **" + maxTemperature + "ºC**\n" + "Min Temperature: **" + minTemperature + "ºC**\n" + "Humidity: **" + humidity + "%**\n")
                    .WithColor(102, 204, 0);

                await ReplyAsync("", false, embed.Build());
            }
            catch (WebException e)
            {
                if (e.Status == WebExceptionStatus.ProtocolError)
                {
                    //Error handling
                    HttpStatusCode code = ((HttpWebResponse)e.Response).StatusCode;
                    if (code == HttpStatusCode.NotFound)
                    {
                        errorBuilder.WithDescription("**City not found!** Please try again.");
                        await ReplyAsync("", false, errorBuilder.Build());
                    }                        
                    if (code == HttpStatusCode.BadRequest)
                    {
                        errorBuilder.WithDescription("**City not supported!**");
                        await ReplyAsync("", false, errorBuilder.Build());
                    }                        
                }
            }
        }
    }
}