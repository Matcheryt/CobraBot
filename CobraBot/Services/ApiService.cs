using System;
using System.Net;
using System.Threading.Tasks;
using CobraBot.Handlers;
using CobraBot.Helpers;
using Discord;
using Discord.Commands;
using Newtonsoft.Json.Linq;

namespace CobraBot.Services
{
    public sealed class ApiService
    {
        /* API Documentation
         * Steam: https://developer.valvesoftware.com/wiki/Steam_Web_API
         * OpenWeatherMap: https://openweathermap.org/api
         * Oxford Dictionary: https://developer.oxforddictionaries.com/documentation */

        #region ApiKeys

        private readonly string _dictApiKey;
        private readonly string _dictAppId;
        private readonly string _steamDevKey;
        private readonly string _owmApiKey;

        public ApiService()
        {
            _dictApiKey = Configuration.ReturnSavedValue("APIKEYS", "OxfordDictionary");
            _dictAppId = Configuration.ReturnSavedValue("APIKEYS", "OxfordAppId");
            _steamDevKey = Configuration.ReturnSavedValue("APIKEYS", "Steam");
            _owmApiKey = Configuration.ReturnSavedValue("APIKEYS", "OWM");
        }
        #endregion

        public async Task<Embed> SearchDictionaryAsync(string wordToSearch, SocketCommandContext context)
        {
            try
            {
                //Make request with necessary headers
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://od-api.oxforddictionaries.com/api/v2/entries/en-gb/" + wordToSearch.ToLower() + "?strictMatch=false");
                request.Method = "GET";
                request.ContinueTimeout = 12000;
                request.Accept = "application/json";
                request.Headers["app_id"] = _dictAppId;
                request.Headers["app_key"] = _dictApiKey;

                string json = await Helper.HttpRequestAndReturnJson(request);

                JObject jsonParsed = JObject.Parse(json);

                JToken wordDefinition = null, wordExample = null, synonyms = null;

                try
                {
                    wordDefinition = jsonParsed["results"][0]["lexicalEntries"][0]["entries"][0]["senses"][0]["definitions"][0];
                    wordExample = jsonParsed["results"][0]["lexicalEntries"][0]["entries"][0]["senses"][0]["examples"][0]["text"];
                    synonyms = jsonParsed["results"][0]["lexicalEntries"][1]["entries"][0]["senses"][0]["synonyms"][0]["text"];

                    return await Helper.CreateBasicEmbed($"{Helper.FirstLetterToUpper(wordToSearch)} meaning", "**Definition:\n  **" + wordDefinition + "\n**Example:\n  **" + wordExample + "\n**Synonyms:**\n  " + synonyms, Color.DarkMagenta);
                }
                catch (Exception)
                {
                    if (wordDefinition == null)
                        wordDefinition = "No definition found.";

                    if (wordExample == null)
                        wordExample = "No example found.";

                    if (synonyms == null)
                        synonyms = "No synonyms found.";

                    return await Helper.CreateBasicEmbed($"{Helper.FirstLetterToUpper(wordToSearch)} meaning", "**Definition:\n  **" + wordDefinition + "\n**Example:\n  **" + wordExample + "\n**Synonyms:**\n  " + synonyms, Color.DarkMagenta);
                }
            }
            catch (Exception e)
            {
                WebException webException = (WebException)e;
                if (webException.Status == WebExceptionStatus.ProtocolError)
                {
                    if (((HttpWebResponse)webException.Response).StatusCode == HttpStatusCode.NotFound)
                    {
                        return await Helper.CreateErrorEmbed("Word requested not found.");
                    }
                    if (((HttpWebResponse)webException.Response).StatusCode == HttpStatusCode.BadRequest)
                    {
                        return await Helper.CreateErrorEmbed("Word not supported.");
                    }
                }

                return await Helper.CreateErrorEmbed($"An error occurred\n{e.Message}");
            }
        }

        #region SteamMethods
        public async Task<Embed> GetSteamInfoAsync(string userId)
        {
            string steamId64;

            //Variables
            string steamName, realName, avatarUrl, profileUrl, countryCode, steamUserLevel;
            int onlineStatusGet, profileVisibilityGet;

            //Not the best way to verify if user is inputing the vanityURL or the SteamID, but it works
            //Verify if steam ID contains only numbers and is less than 17 digits long (steamID64 length)
            if (!Helper.IsDigitsOnly(userId) && userId.Length < 17)
            {
                //If not, get steam id 64 based on user input
                steamId64 = await GetSteamId64(userId);
                if (steamId64 == "User not found")
                    return await Helper.CreateErrorEmbed("**User not found!** Please check your SteamID and try again.");
            }
            else
            {
                //If it is digits only and it's length is 17 digits long, then assume the user input is the steam 64 id of a steam profile
                steamId64 = userId;
            }

            steamUserLevel = await GetSteamLevel(steamId64);

            try
            {
                //Create web request, requesting player profile info                
                var request = (HttpWebRequest)WebRequest.Create("https://api.steampowered.com/ISteamUser/GetPlayerSummaries/v2/?key=" + _steamDevKey + "&steamids=" + steamId64);

                var httpResponse = await Helper.HttpRequestAndReturnJson(request);

                //Parse the json from httpResponse
                var profileJsonResponse = JObject.Parse(httpResponse);

                //Give values to the variables
                try
                {
                    steamName = (string)profileJsonResponse["response"]["players"][0]["personaname"];
                    realName = (string)profileJsonResponse["response"]["players"][0]["realname"];
                    avatarUrl = (string)profileJsonResponse["response"]["players"][0]["avatarfull"];
                    onlineStatusGet = (int)profileJsonResponse["response"]["players"][0]["personastate"];
                    profileUrl = (string)profileJsonResponse["response"]["players"][0]["profileurl"];
                    countryCode = (string)profileJsonResponse["response"]["players"][0]["loccountrycode"];
                    profileVisibilityGet = (int)profileJsonResponse["response"]["players"][0]["communityvisibilitystate"];
                }
                catch (Exception)
                {
                    return await Helper.CreateErrorEmbed("**User not found!** Please check your SteamID and try again.");
                }
            }
            catch (WebException)
            {
                return await Helper.CreateErrorEmbed("**An error occurred**");
            }

            //Online Status Switch
            string onlineStatus = onlineStatusGet switch
            {
                0 => "Offline",
                1 => "Online",
                2 => "Busy",
                3 => "Away",
                4 => "Snooze",
                5 => "Looking to Trade",
                6 => "Looking to Play",
                _ => null
            };

            //Profile Visibility Switch
            string profileVisibility = profileVisibilityGet switch
            {
                1 => "Private",
                2 => "Friends Only",
                3 => "Public",
                _ => null
            };

            //If one of the variables is null, assign "Not found" to their value
            realName ??= "Not found";
            countryCode ??= "Not found";

            var embed = new EmbedBuilder();
            embed.WithTitle(steamName + " Steam Info")
                .WithDescription("\n**Steam Name:** " + steamName + "\n**Steam Level:** " + steamUserLevel + "\n**Real Name:** " + realName + "\n**Steam ID64:** " + steamId64 + "\n**Status:** " + onlineStatus + "\n**Profile Privacy:** " + profileVisibility + "\n**Country:** " + countryCode + "\n\n" + profileUrl)
                .WithThumbnailUrl(avatarUrl)
                .WithColor(Color.Blue);

            return embed.Build();
        }

        /// <summary>Retrieve steam id 64 based on userId.
        /// <para>Used to retrieve a valid steamId64 based on a vanity url.</para>
        /// </summary>
        private async Task<string> GetSteamId64(string userId)
        {
            //Create request
            var request = (HttpWebRequest)WebRequest.Create("http://api.steampowered.com/ISteamUser/ResolveVanityURL/v0001/?key=" + _steamDevKey + "&vanityurl=" + userId);

            string httpResponse = await Helper.HttpRequestAndReturnJson(request);

            //Save steamResponse in a string and then retrieve user steamId64
            try
            {
                var jsonParsed = JObject.Parse(httpResponse);

                var userIdResolved = jsonParsed["response"]["steamid"].ToString();

                return userIdResolved;
            }
            catch (Exception)
            {
                return "User not found";
            }
        }

        /// <summary>Retrieve steam level based on userId.
        /// <para>Used to retrieve the level of an account based on a valid steamId64.</para>
        /// </summary>
        private async Task<string> GetSteamLevel(string userId)
        {
            //Create a webRequest to steam api endpoint
            var request = (HttpWebRequest)WebRequest.Create("http://api.steampowered.com/IPlayerService/GetSteamLevel/v1/?key=" + _steamDevKey + "&steamid=" + userId);

            string httpResponse = await Helper.HttpRequestAndReturnJson(request);

            //Save steamResponse in a string and then retrieve user level
            try
            {
                //Parse the json from httpResponse
                var jsonParsed = JObject.Parse(httpResponse);

                string userLevel = jsonParsed["response"]["player_level"].ToString();

                return userLevel;
            }
            catch (Exception)
            {
                return "Not found";
            }
        }
        #endregion

        public string Lmgtfy([Remainder] string textToSearch)
        {
            if (textToSearch.Contains(" "))
                textToSearch = textToSearch.Replace(" ", "+");

            return $"https://lmgtfy.app/?q=" + textToSearch;
        }

        public async Task<Embed> GetWeatherAsync([Remainder]string city)
        {
            try
            {
                //Request weather from OWM and return json
                var request = (HttpWebRequest)WebRequest.Create("http://api.openweathermap.org/data/2.5/weather?q=" + city + "&appid=" + _owmApiKey + "&units=metric");

                string httpResponse = await Helper.HttpRequestAndReturnJson(request);

                //Parse the json from httpResponse
                var weatherParsedJson = JObject.Parse(httpResponse);

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

                return embed.Build();
            }
            catch (Exception e)
            {
                var webException = (WebException)e;
                if (webException.Status == WebExceptionStatus.ProtocolError)
                {
                    //Error handling
                    if (((HttpWebResponse)webException.Response).StatusCode == HttpStatusCode.NotFound)
                    {
                        return await Helper.CreateErrorEmbed("**City not found!** Please try again.");
                    }
                    if (((HttpWebResponse)webException.Response).StatusCode == HttpStatusCode.BadRequest)
                    {
                        return await Helper.CreateErrorEmbed("**Not supported!**");
                    }
                }

                return await Helper.CreateErrorEmbed($"An error occurred\n{e.Message}");
            }
        }
    }
}
