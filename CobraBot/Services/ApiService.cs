﻿using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using CobraBot.Common;
using CobraBot.Common.EmbedFormats;
using CobraBot.Common.Json_Models;
using CobraBot.Handlers;
using CobraBot.Helpers;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CobraBot.Services
{
    public sealed class ApiService
    {
        /* API Documentation
         * Steam: https://developer.valvesoftware.com/wiki/Steam_Web_API
         * OpenWeatherMap: https://openweathermap.org/api
         * Oxford Dictionary: https://developer.oxforddictionaries.com/documentation */

        private readonly IMemoryCache _memoryCache;
        
        public ApiService(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        /// <summary>Retrieve specified word definition from Oxford Dictionary.
        /// </summary>
        public async Task<Embed> SearchDictionaryAsync(string wordToSearch)
        {
            //If we have a response cached, then return that response
            if (_memoryCache.TryGetValue($"DICTIONARY{wordToSearch}", out Embed savedResponse))
                return savedResponse;
            
            string jsonResponse;

            try
            {
                //Make request with necessary headers
                var request = new HttpRequestMessage()
                {
                    RequestUri =
                        new Uri(
                            $"https://od-api.oxforddictionaries.com/api/v2/entries/en-gb/{wordToSearch}?strictMatch=false"),
                    Method = HttpMethod.Get,
                    Headers =
                    {
                        {"app_id", Configuration.DictAppId},
                        {"app_key", Configuration.DictApiKey}
                    }
                };

                jsonResponse = await Helper.HttpRequestAndReturnJson(request);
            }
            catch (Exception e)
            {
                var httpException = (HttpRequestException)e;

                //Error handling
                return httpException.StatusCode switch
                {
                    //If not found
                    HttpStatusCode.NotFound => CustomFormats.CreateErrorEmbed(
                        "**Word not found!** Please try again."),

                    //If bad request
                    HttpStatusCode.BadRequest => CustomFormats.CreateErrorEmbed(
                        "**Not supported!** Please try again."),

                    //Default error message
                    _ => CustomFormats.CreateErrorEmbed($"An error occurred\n{e.Message}")
                };
            }

            var jsonParsed = JObject.Parse(jsonResponse);

            JToken wordDefinition = null, wordExample = null, synonyms = null;

            try
            {
                wordDefinition = jsonParsed["results"][0]["lexicalEntries"][0]["entries"][0]["senses"][0]["definitions"][0];
                wordExample = jsonParsed["results"][0]["lexicalEntries"][0]["entries"][0]["senses"][0]["examples"][0]["text"];
                synonyms = jsonParsed["results"][0]["lexicalEntries"][1]["entries"][0]["senses"][0]["synonyms"][0]["text"];

                return CustomFormats.CreateBasicEmbed($"{Helper.FirstLetterToUpper(wordToSearch)} meaning", "**Definition:\n  **" + wordDefinition + "\n**Example:\n  **" + wordExample + "\n**Synonyms:**\n  " + synonyms, Color.DarkMagenta);
            }
            catch (Exception)
            {
                wordDefinition ??= "No definition found.";

                wordExample ??= "No example found.";

                synonyms ??= "No synonyms found.";

                var embed = CustomFormats.CreateBasicEmbed($"{Helper.FirstLetterToUpper(wordToSearch)} meaning",
                    $"**Definition:**\n {wordDefinition}" +
                    $"\n**Example:**\n {wordExample}" +
                    $"\n**Synonyms:**\n {synonyms}",
                    Color.DarkMagenta);

                //Saves response to cache for 24 hours
                _memoryCache.Set($"DICTIONARY{wordToSearch}", embed, TimeSpan.FromHours(24));

                return embed;
            }
        }


        #region SteamMethods
        /// <summary>Retrieve steam profile from specified user.
        /// </summary>
        public async Task<Embed> GetSteamInfoAsync(string userId)
        {
            //If we have a response cached, then return that response
            if (_memoryCache.TryGetValue($"STEAM{userId}", out Embed savedResponse))
                return savedResponse;
            
            string steamId64;

            //Not the best way to verify if user is inputing the vanityURL or the SteamID, but it works
            //Verify if steam ID contains only numbers and is less than 17 digits long (steamID64 length)
            if (!Helper.IsDigitsOnly(userId) && userId.Length < 17)
            {
                //If not, get steam id 64 based on user input
                steamId64 = await GetSteamId64(userId);
                if (steamId64 == "User not found")
                    return CustomFormats.CreateErrorEmbed("**User not found!** Please check your SteamID and try again.");
            }
            else
            {
                //If it is digits only and it's length is 17 digits long, then assume the user input is the steam 64 id of a steam profile
                steamId64 = userId;
            }

            var steamUserLevel = await GetSteamLevel(steamId64);

            string jsonResponse;

            try
            {
                //Create web request, requesting player profile info                
                var request = new HttpRequestMessage()
                {
                    RequestUri = new Uri($"https://api.steampowered.com/ISteamUser/GetPlayerSummaries/v2/?key={Configuration.SteamDevKey}&steamids={steamId64}"),
                    Method = HttpMethod.Get
                };

                jsonResponse = await Helper.HttpRequestAndReturnJson(request);
            }
            catch (WebException)
            {
                return CustomFormats.CreateErrorEmbed("**An error occurred**");
            }

            //Deserializes json response
            var profileResponse = JsonConvert.DeserializeObject<Steam>(jsonResponse);

            //If response doesn't return anything, then tell the command issuer that no user was found
            if (!profileResponse.Response.Players.Any())
                return CustomFormats.CreateErrorEmbed("**User not found!** Please check your SteamID and try again.");

            var player = profileResponse.Response.Players[0];

            //Online Status Switch
            var onlineStatus = player.Personastate switch
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
            var profileVisibility = player.Communityvisibilitystate switch
            {
                1 => "Private",
                2 => "Friends Only",
                3 => "Public",
                _ => null
            };

            //If one of the variables is null, assign "Not found" to their value
            player.Realname ??= "Not found";
            player.Loccountrycode ??= "Not found";

            var nameField = new EmbedFieldBuilder().WithName("Steam Name:").WithValue(player.Personaname).WithIsInline(true);
            var realNameField = new EmbedFieldBuilder().WithName("Real Name:").WithValue(player.Realname).WithIsInline(true);
            var levelField = new EmbedFieldBuilder().WithName("Steam Level:").WithValue(steamUserLevel).WithIsInline(true);
            var id64Field = new EmbedFieldBuilder().WithName("Steam ID64").WithValue(steamId64).WithIsInline(true);
            var statusField = new EmbedFieldBuilder().WithName("Status:").WithValue(onlineStatus).WithIsInline(true);
            var visibilityField = new EmbedFieldBuilder().WithName("Profile Privacy:").WithValue(profileVisibility).WithIsInline(true);
            var countryField = new EmbedFieldBuilder().WithName("Country:").WithValue(player.Loccountrycode).WithIsInline(true);

            var embed = new EmbedBuilder();
            embed.WithTitle($"{CustomEmotes.SteamEmote}  {player.Personaname}")
                .WithUrl(player.Profileurl)
                .WithFields(nameField, realNameField, levelField, statusField, visibilityField, countryField, id64Field)
                //.WithFooter(player.)
                .WithThumbnailUrl(player.Avatarfull)
                .WithColor(0x2a475e);

            //Save the response to cache for 30 seconds
            _memoryCache.Set($"STEAM{userId}", embed.Build(), TimeSpan.FromSeconds(30));
            
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



        /// <summary>Retrieve weather from specified city using OWM.
        /// </summary>
        public async Task<Embed> GetWeatherAsync([Remainder]string city)
        {
            //If we have a response cached, then return that response
            if (_memoryCache.TryGetValue($"WEATHER{city}", out Embed savedResponse))
                return savedResponse;
            
            //Request weather from OWM and return json
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri($"http://api.openweathermap.org/data/2.5/weather?q={city}&appid={Configuration.OwmApiKey}&units=metric"),
                Method = HttpMethod.Get,
            };

            string jsonResponse;

            try
            {
                jsonResponse = await Helper.HttpRequestAndReturnJson(request);
            }
            catch (Exception e)
            {
                var httpException = (HttpRequestException)e;

                //Error handling
                return httpException.StatusCode switch
                {
                    //If not found
                    HttpStatusCode.NotFound => CustomFormats.CreateErrorEmbed("**City not found!** Please try again."),

                    //If bad request
                    HttpStatusCode.BadRequest => CustomFormats.CreateErrorEmbed("**Not supported!**"),

                    //Default error message
                    _ => CustomFormats.CreateErrorEmbed($"An error occurred\n{e.Message}")
                };
            }

            //Deserializes json response
            var weatherResponse = JsonConvert.DeserializeObject<Owm>(jsonResponse);
            
            var thumbnailUrl = "http://openweathermap.org/img/w/" + weatherResponse.Weather[0].Icon + ".png";

            //Send message with the current weather
            var embed = new EmbedBuilder();
            embed.WithTitle("Current Weather at " + weatherResponse.Name + ", " + weatherResponse.Sys.Country)
                .WithThumbnailUrl(thumbnailUrl)
                .WithDescription("**Conditions: " + weatherResponse.Weather[0].Main + ", " +
                                 Helper.FirstLetterToUpper(weatherResponse.Weather[0].Description) +
                                 "**\nTemperature: **" + weatherResponse.Main.Temp + "ºC**\n" + "Max Temperature: **" +
                                 weatherResponse.Main.TempMax + "ºC**\n" + "Min Temperature: **" +
                                 weatherResponse.Main.TempMin + "ºC**\n" + "Humidity: **" +
                                 weatherResponse.Main.Humidity + "%**\n")
                .WithColor(0xe96d49);

            //Save the response to cache for 5 minutes
            _memoryCache.Set($"WEATHER{city}", embed.Build(), TimeSpan.FromMinutes(5));

            return embed.Build();
        }


        /// <summary>Retrieve specified movie/tv show info from OMDB.
        /// </summary>
        public async Task<Embed> GetOmdbInformation(string type, [Remainder] string show)
        {
            //If we have a response cached, then return that response
            if (_memoryCache.TryGetValue($"OMDB{show}", out Embed savedResponse))
                return savedResponse;

            if (type != "movie" && type != "episode" && type != "series")
                return CustomFormats.CreateErrorEmbed(
                    "**Invalid type!** Valid types are `movie`, `series` and `episode`");

            //Request weather from OWM and return json
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri($"https://omdbapi.com/?apikey={Configuration.OmdbApiKey}&t={show}&r=json&type={type}"),
                Method = HttpMethod.Get,
            };

            string jsonResponse;

            try
            {
                jsonResponse = await Helper.HttpRequestAndReturnJson(request);
            }
            catch (Exception e)
            {
                var httpException = (HttpRequestException)e;

                //Error handling
                return httpException.StatusCode switch
                {
                    //If not found
                    HttpStatusCode.NotFound => CustomFormats.CreateErrorEmbed("**Show not found!** Please try again."),

                    //If bad request
                    HttpStatusCode.BadRequest => CustomFormats.CreateErrorEmbed("**Not supported!**"),

                    //Default error message
                    _ => CustomFormats.CreateErrorEmbed($"An error occurred\n{e.Message}")
                };
            }

            Omdb omdbResponse;

            try
            {
                //Tries to deserialize json response
                omdbResponse = JsonConvert.DeserializeObject<Omdb>(jsonResponse);
            }
            catch(Exception)
            {
                //If an exception occurs, for example because the show wasn't found, then let the user know
                return CustomFormats.CreateErrorEmbed($"{show} not found!");
            }

            var imdbRatingField = new EmbedFieldBuilder().WithName($"{CustomEmotes.ImdbEmote}  IMDB Rating").WithValue($"{omdbResponse.ImdbRating} ({omdbResponse.ImdbVotes} votes)").WithIsInline(true);
            var metascoreField = new EmbedFieldBuilder().WithName($"{CustomEmotes.MetascoreEmote}  Metascore").WithValue(omdbResponse.Metascore).WithIsInline(true);

            //Create embed
            var embed = new EmbedBuilder()
                .WithTitle($"{omdbResponse.Title} | {omdbResponse.Year}")
                .WithThumbnailUrl(omdbResponse.Poster)
                .WithUrl($"https://www.imdb.com/title/{omdbResponse.ImdbId}")
                .WithFooter($"{Helper.FirstLetterToUpper(omdbResponse.Type)} | {omdbResponse.Genre}")
                .WithColor(0xDBA506)
                .WithDescription(
                    $"**Writers:** {omdbResponse.Writer}\n**Actors:** {omdbResponse.Actors}\n**Language**: {omdbResponse.Language}\n\n**Plot:** {omdbResponse.Plot}")
                .WithFields(imdbRatingField, metascoreField);

            foreach (var rating in omdbResponse.Ratings.Where(rating => rating.Source != "Internet Movie Database" && rating.Source != "Metacritic"))
            {
                embed.AddField(x =>
                {
                    x.Name = rating.Source == "Rotten Tomatoes" ? $"{CustomEmotes.RottenTomatoesEmote}  {rating.Source}" : rating.Source;
                    x.Value = rating.Value;
                    x.IsInline = true;
                });
            }

            //Save the response to cache for 24 hours
            _memoryCache.Set($"OMDB{show}", embed.Build(), TimeSpan.FromHours(24));

            return embed.Build();
        }
    }
}
