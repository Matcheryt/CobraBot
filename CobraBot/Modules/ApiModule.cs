using Discord.Commands;
using Discord;
using System.Threading.Tasks;
using System;
using System.Net;
using Newtonsoft.Json.Linq;
using CobraBot.Helpers;

namespace CobraBot.Modules
{
    public class ApiModule : ModuleBase<SocketCommandContext>
    {

        /* API Documentation
         * Steam: https://developer.valvesoftware.com/wiki/Steam_Web_API
         * OpenWeatherMap: https://openweathermap.org/api
         * Oxford Dictionary: https://developer.oxforddictionaries.com/documentation */

        #region ApiKeys
        readonly string dictApiKey;
        readonly string dictAppId;
        readonly string steamDevKey;
        readonly string owmApiKey;

        //Constructor initializing API key strings from config file
        private ApiModule()
        {
            dictApiKey = Configuration.ReturnSavedValue("APIKEYS", "OxfordDictionary");
            dictAppId = Configuration.ReturnSavedValue("APIKEYS", "OxfordAppId");
            steamDevKey = Configuration.ReturnSavedValue("APIKEYS", "Steam");
            owmApiKey = Configuration.ReturnSavedValue("APIKEYS", "OWM");
        }
        #endregion


        //Dictionary Command
        [Command("dict", RunMode = RunMode.Async)]
        public async Task SearchDictionary(string wordToSearch)
        {
            try
            {
            
                //Make request with necessary headers
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://od-api.oxforddictionaries.com/api/v2/entries/en-gb/" + wordToSearch.ToLower() + "?strictMatch=false");
                request.Method = "GET";
                request.ContinueTimeout = 12000;
                request.Accept = "application/json";
                request.Headers["app_id"] = dictAppId;
                request.Headers["app_key"] = dictApiKey;

                string json = await Helper.HttpRequestAndReturnJson(request);

                JObject jsonParsed = JObject.Parse(json);
                var embed = new EmbedBuilder();

                JToken wordDefinition = null, wordExample = null, synonyms = null;

                try
                {
                    wordDefinition = jsonParsed["results"][0]["lexicalEntries"][0]["entries"][0]["senses"][0]["definitions"][0];
                    wordExample = jsonParsed["results"][0]["lexicalEntries"][0]["entries"][0]["senses"][0]["examples"][0]["text"];
                    synonyms = jsonParsed["results"][0]["lexicalEntries"][1]["entries"][0]["senses"][0]["synonyms"][0]["text"];

                    embed.WithTitle(Helper.FirstLetterToUpper(wordToSearch) + " Meaning")
                        .WithDescription("**Definition:\n  **" + wordDefinition + "\n**Example:\n  **" + wordExample + "\n**Synonyms**\n  " + synonyms)
                        .WithColor(Color.DarkMagenta);
                }
                catch (Exception)
                {
                    if (wordDefinition == null)
                        wordDefinition = "No definition found.";

                    if (wordExample == null)
                        wordExample = "No example found.";

                    if (synonyms == null)
                        synonyms = "No synonyms found.";

                    embed.WithTitle(Helper.FirstLetterToUpper(wordToSearch) + " Meaning")
                        .WithDescription("**Definition:\n  **" + wordDefinition + "\n**Example:\n  **" + wordExample + "\n**Synonyms**\n  " + synonyms)
                        .WithColor(Color.DarkMagenta);
                }                

                await ReplyAsync("", false, embed.Build());
            }
            catch (WebException e)
            {
                if (e.Status == WebExceptionStatus.ProtocolError)
                {
                    if (((HttpWebResponse)e.Response).StatusCode == HttpStatusCode.NotFound)
                    {
                        await ReplyAsync(embed: await Helper.CreateErrorEmbed("Word requested not found."));
                    }
                    if (((HttpWebResponse)e.Response).StatusCode == HttpStatusCode.BadRequest)
                    {
                        await ReplyAsync(embed: await Helper.CreateErrorEmbed("Word not supported."));
                    }
                }
            }
        }

        #region SteamAPI

        //API that returns information about a steam user
        [Command("steam", RunMode = RunMode.Async)]
        public async Task GetSteamInfo(string userId)
        {
            string id64response;

            //Variables
            string steamName;
            string realName;
            ulong steam64ID;
            string avatarUrl;
            int onlineStatusGet;
            string profileUrl;
            string countryCode;
            int profileVisibilityGet;
            int steamUserLevel;

            //Not the best way to verify if user is inputing the vanityURL or the SteamID, but it works
            //Verify if steam ID contains only numbers
            if (!Helper.IsDigitsOnly(userId))
            {
                //If not, get steam id 64 based on user input
                id64response = await GetSteamId64(userId);
            }
            else
            {
                //If it is digits only, then assume the user input is the steam 64 id of a steam profile
                id64response = userId;
            }

            steamUserLevel = await GetSteamLevel(id64response);

            try
            {
                //Create web request, requesting player profile info                
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://api.steampowered.com/ISteamUser/GetPlayerSummaries/v2/?key=" + steamDevKey + "&steamids=" + id64response);
                
                string httpResponse = await Helper.HttpRequestAndReturnJson(request);

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
                    await ReplyAsync(embed: await Helper.CreateErrorEmbed("**User not found!** Please check your SteamID and try again."));
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
                await ReplyAsync(embed: await Helper.CreateErrorEmbed("**An error ocurred**"));
            }
        }


        /* If user input on "steam" command isn't the steamId64 but instead the vanity url, then
        create a request that will return the steamId64 based on the vanity url*/
        /// <summary>Retrieve steam id 64 based on userId.
        /// <para>Used to retrieve a valid steamId64 based on a vanity url.</para>
        /// </summary>
        public async Task<string> GetSteamId64(string userId)
        {
            TaskCompletionSource<String> tcs = new TaskCompletionSource<String>();

            string userIdResolved;

            //Create request
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://api.steampowered.com/ISteamUser/ResolveVanityURL/v0001/?key=" + steamDevKey + "&vanityurl=" + userId);
            
            string httpResponse = await Helper.HttpRequestAndReturnJson(request);

            //Save steamResponse in a string and then retrieve user steamId64
            try
            {
                JObject jsonParsed = JObject.Parse(httpResponse);

                userIdResolved = jsonParsed["response"]["steamid"].ToString();

                tcs.SetResult(userIdResolved);
            }
            catch (Exception)
            {
                await ReplyAsync(embed: await Helper.CreateErrorEmbed("**User not found!** Please check your SteamID and try again."));
            }

            string result = await tcs.Task;

            return result;
        }


        //Get steam level
        /// <summary>Retrieve steam level based on userId.
        /// <para>Used to retrieve the level of an account based on a valid steamId64.</para>
        /// </summary>
        public async Task<int> GetSteamLevel(string userId)
        {
            TaskCompletionSource<int> tcs = new TaskCompletionSource<int>();

            int userLevel = 0;

            //Create a webRequest to steam api endpoint
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://api.steampowered.com/IPlayerService/GetSteamLevel/v1/?key=" + steamDevKey + "&steamid=" + userId);

            string httpResponse = await Helper.HttpRequestAndReturnJson(request);

            //Save steamResponse in a string and then retrieve user level
            try
            {
                //Parse the json from httpResponse
                JObject jsonParsed = JObject.Parse(httpResponse);

                userLevel = (int)jsonParsed["response"]["player_level"];

                tcs.SetResult(userLevel);
            }
            catch (Exception)
            {
                await ReplyAsync(embed: await Helper.CreateErrorEmbed("**Couldn't fetch level**"));
            }

            int result = await tcs.Task;

            return result;
        }

        #endregion

        //Generate lmgtfy link
        [Command("lmgtfy")]
        public async Task Lmgtfy([Remainder] string textToSearch)
        {
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
                await ReplyAsync("An error ocurred");
            }
        }


        //Get weather based on user input
        [Command("weather", RunMode = RunMode.Async)]
        public async Task Weather([Remainder] string city)
        {

            try
            {
                //Request weather from OWM and return json
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://api.openweathermap.org/data/2.5/weather?q=" + city + "&appid=" + owmApiKey + "&units=metric");

                string httpResponse = await Helper.HttpRequestAndReturnJson(request);

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

                weatherDescription = Helper.FirstLetterToUpper(weatherDescription);

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
                    if (((HttpWebResponse)e.Response).StatusCode == HttpStatusCode.NotFound)
                    {
                        await ReplyAsync(embed: await Helper.CreateErrorEmbed("**City not found!** Please try again."));
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