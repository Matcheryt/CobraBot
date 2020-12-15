using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using CobraBot.Common;
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

        public async Task<Embed> SearchDictionaryAsync(string wordToSearch, SocketCommandContext context)
        {
            try
            {
                //Make request with necessary headers
                var request = new HttpRequestMessage()
                {
                    RequestUri = new Uri($"https://od-api.oxforddictionaries.com/api/v2/entries/en-gb/{wordToSearch}?strictMatch=false"),
                    Method = HttpMethod.Get,
                    Headers =
                    {
                        { "app_id", Configuration.DictAppId },
                        { "app_key", Configuration.DictApiKey }
                    }
                };

                string json = await Helper.HttpRequestAndReturnJson(request);

                JObject jsonParsed = JObject.Parse(json);

                JToken wordDefinition = null, wordExample = null, synonyms = null;

                try
                {
                    wordDefinition = jsonParsed["results"][0]["lexicalEntries"][0]["entries"][0]["senses"][0]["definitions"][0];
                    wordExample = jsonParsed["results"][0]["lexicalEntries"][0]["entries"][0]["senses"][0]["examples"][0]["text"];
                    synonyms = jsonParsed["results"][0]["lexicalEntries"][1]["entries"][0]["senses"][0]["synonyms"][0]["text"];

                    return EmbedFormats.CreateBasicEmbed($"{Helper.FirstLetterToUpper(wordToSearch)} meaning", "**Definition:\n  **" + wordDefinition + "\n**Example:\n  **" + wordExample + "\n**Synonyms:**\n  " + synonyms, Color.DarkMagenta);
                }
                catch (Exception)
                {
                    wordDefinition ??= "No definition found.";

                    wordExample ??= "No example found.";

                    synonyms ??= "No synonyms found.";

                    return EmbedFormats.CreateBasicEmbed($"{Helper.FirstLetterToUpper(wordToSearch)} meaning", "**Definition:\n  **" + wordDefinition + "\n**Example:\n  **" + wordExample + "\n**Synonyms:**\n  " + synonyms, Color.DarkMagenta);
                }
            }
            catch (Exception e)
            {
                var httpException = (HttpRequestException)e;

                //Error handling
                return httpException.StatusCode switch
                {
                    //If not found
                    HttpStatusCode.NotFound => EmbedFormats.CreateErrorEmbed("**Word not found!** Please try again."),

                    //If bad request
                    HttpStatusCode.BadRequest => EmbedFormats.CreateErrorEmbed("**Not supported!** Please try again."),

                    //Default error message
                    _ => EmbedFormats.CreateErrorEmbed($"An error occurred\n{e.Message}")
                };
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
                    return EmbedFormats.CreateErrorEmbed("**User not found!** Please check your SteamID and try again.");
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
                var request = new HttpRequestMessage()
                {
                    RequestUri = new Uri($"https://api.steampowered.com/ISteamUser/GetPlayerSummaries/v2/?key={Configuration.SteamDevKey}&steamids={steamId64}"),
                    Method = HttpMethod.Get
                };

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
                    return EmbedFormats.CreateErrorEmbed("**User not found!** Please check your SteamID and try again.");
                }
            }
            catch (WebException)
            {
                return EmbedFormats.CreateErrorEmbed("**An error occurred**");
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
        private static async Task<string> GetSteamId64(string userId)
        {
            //Create request
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri($"http://api.steampowered.com/ISteamUser/ResolveVanityURL/v0001/?key={Configuration.SteamDevKey}&vanityurl={userId}"),
                Method = HttpMethod.Get,
            };

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
        private static async Task<string> GetSteamLevel(string userId)
        {
            //Create a webRequest to steam api endpoint
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri($"http://api.steampowered.com/IPlayerService/GetSteamLevel/v1/?key={Configuration.SteamDevKey}&steamid={userId}"),
                Method = HttpMethod.Get,
            };

            //Save steamResponse in a string and then retrieve user level
            try
            {
                //Parse the json from httpResponse
                var jsonParsed = JObject.Parse(await Helper.HttpRequestAndReturnJson(request));

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

            return $"https://lmgtfy.app/?q={textToSearch}";
        }

        public async Task<Embed> GetWeatherAsync([Remainder]string city)
        {
            //Request weather from OWM and return json
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri($"http://api.openweathermap.org/data/2.5/weather?q={city}&appid={Configuration.OwmApiKey}&units=metric"),
                Method = HttpMethod.Get,
            };

            try
            {
                //Parse the json from httpResponse
                var weatherParsedJson = JObject.Parse(await Helper.HttpRequestAndReturnJson(request));

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
                var httpException = (HttpRequestException)e;

                //Error handling
                return httpException.StatusCode switch
                {
                    //If not found
                    HttpStatusCode.NotFound => EmbedFormats.CreateErrorEmbed("**City not found!** Please try again."),
                    
                    //If bad request
                    HttpStatusCode.BadRequest => EmbedFormats.CreateErrorEmbed("**Not supported!**"),
                    
                    //Default error message
                    _ => EmbedFormats.CreateErrorEmbed($"An error occurred\n{e.Message}")
                };
            }
        }
    }
}
