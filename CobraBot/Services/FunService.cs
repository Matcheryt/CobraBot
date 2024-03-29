﻿/*
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
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using CobraBot.Common.EmbedFormats;
using CobraBot.Common.Json_Models.KSoft;
using CobraBot.Handlers;
using CobraBot.Helpers;
using Discord;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CobraBot.Services
{
    public sealed class FunService
    {
        /// <summary>
        ///     Retrieves a random meme from KSoft.Si database.
        /// </summary>
        public static async Task<Embed> GetRandomMemeAsync(bool channelIsNsfw)
        {
            //Create request to specified url
            var request = new HttpRequestMessage
            {
                RequestUri = new Uri("https://api.ksoft.si/images/random-meme"),
                Method = HttpMethod.Get,
                Headers =
                {
                    { "Authorization", $"Bearer {Configuration.KSoftApiKey}" }
                }
            };

            try
            {
                var jsonResponse = await HttpHelper.HttpRequestAndReturnJson(request);

                //Deserialize json response
                var meme = JsonConvert.DeserializeObject<KSoftReddit>(jsonResponse);

                if (!channelIsNsfw && meme.Nsfw)
                    return CustomFormats.CreateErrorEmbed("NSFW isn't enabled on this channel!");

                var embed = new EmbedBuilder()
                    .WithTitle($"{meme.Title}")
                    .WithImageUrl(meme.ImageUrl)
                    .WithColor(Color.DarkBlue)
                    .WithFooter($"{meme.Subreddit}  •  {meme.Author}  |  Powered by KSoft.Si")
                    .WithUrl(meme.Source).Build();

                return embed;
            }
            catch (Exception e)
            {
                return CustomFormats.CreateErrorEmbed(e.Message);
            }
        }


        /// <summary>
        ///     Retrieves a random WikiHow post from KSoft.Si database.
        /// </summary>
        public static async Task<Embed> GetRandomWikiHowAsync(bool channelIsNsfw)
        {
            try
            {
                //Create request to specified url
                var request = new HttpRequestMessage
                {
                    RequestUri = new Uri("https://api.ksoft.si/images/random-wikihow"),
                    Method = HttpMethod.Get,
                    Headers =
                    {
                        { "Authorization", $"Bearer {Configuration.KSoftApiKey}" }
                    }
                };

                var httpResponse = await HttpHelper.HttpRequestAndReturnJson(request);

                var jsonParsed = JObject.Parse(httpResponse);

                var title = (string)jsonParsed["title"];
                var url = (string)jsonParsed["url"];
                var articleUrl = (string)jsonParsed["article_url"];
                var nsfw = (bool)jsonParsed["nsfw"];

                if (!channelIsNsfw && nsfw)
                    return CustomFormats.CreateErrorEmbed("NSFW isn't enabled on this channel!");

                var embed = new EmbedBuilder()
                    .WithTitle(title)
                    .WithImageUrl(url)
                    .WithColor(Color.DarkBlue)
                    .WithFooter("Powered by KSoft.Si")
                    .WithUrl(articleUrl).Build();

                return embed;
            }
            catch (Exception e)
            {
                return CustomFormats.CreateErrorEmbed(e.Message);
            }
        }


        /// <summary>
        ///     Retrieves a random cute image/gif from KSoft.Si database.
        /// </summary>
        public static async Task<Embed> GetRandomCuteAsync(bool channelIsNsfw)
        {
            try
            {
                //Create request to specified url
                var request = new HttpRequestMessage
                {
                    RequestUri = new Uri("https://api.ksoft.si/images/random-aww"),
                    Method = HttpMethod.Get,
                    Headers =
                    {
                        { "Authorization", $"Bearer {Configuration.KSoftApiKey}" }
                    }
                };

                var jsonResponse = await HttpHelper.HttpRequestAndReturnJson(request);

                //Deserialize json response
                var cute = JsonConvert.DeserializeObject<KSoftReddit>(jsonResponse);

                if (!channelIsNsfw && cute.Nsfw)
                    return CustomFormats.CreateErrorEmbed("NSFW isn't enabled on this channel!");

                var embed = new EmbedBuilder()
                    .WithTitle(cute.Title)
                    .WithImageUrl(cute.ImageUrl)
                    .WithColor(Color.DarkBlue)
                    .WithFooter($"{cute.Subreddit}  •  {cute.Author}  |  Powered by KSoft.Si")
                    .WithUrl(cute.Source).Build();

                return embed;
            }
            catch (Exception e)
            {
                return CustomFormats.CreateErrorEmbed(e.Message);
            }
        }


        /// <summary>
        ///     Retrieves a random post from specified subreddit.
        /// </summary>
        public static async Task<Embed> GetRandomPostAsync(string subreddit, string span = "week")
        {
            string[] availableSpans = { "hour", "day", "week", "month", "year", "all" };

            if (!availableSpans.Contains(span))
                return CustomFormats.CreateErrorEmbed(
                    $"Invalid span `{span}`. Span can be `hour`, `day`, `week`, `month`, `year` and `all`");

            try
            {
                //Create request to specified url
                var request = new HttpRequestMessage
                {
                    RequestUri =
                        new Uri($"https://api.ksoft.si/images/rand-reddit/{subreddit}?span={span}&remove_nsfw=true"),
                    Method = HttpMethod.Get,
                    Headers =
                    {
                        { "Authorization", $"Bearer {Configuration.KSoftApiKey}" }
                    }
                };

                var jsonResponse = await HttpHelper.HttpRequestAndReturnJson(request);

                //Deserialize json response
                var randomPost = JsonConvert.DeserializeObject<KSoftReddit>(jsonResponse);

                var embed = new EmbedBuilder()
                    .WithTitle(randomPost.Title)
                    .WithImageUrl(randomPost.ImageUrl)
                    .WithColor(Color.DarkBlue)
                    .WithFooter($"{randomPost.Subreddit}  •  {randomPost.Author}  |  Powered by KSoft.Si")
                    .WithUrl(randomPost.Source).Build();

                return embed;
            }
            catch (Exception e)
            {
                var httpException = (HttpRequestException)e;
                return CustomFormats.CreateErrorEmbed(httpException.StatusCode == HttpStatusCode.NotFound
                    ? "**Subreddit not found**"
                    : e.Message);
            }
        }


        /// <summary>
        ///     Retrieves a random image from KSoft.Si database according to the specified tag.
        /// </summary>
        public static async Task<Embed> GetImageFromTagAsync(string tag)
        {
            try
            {
                //Create request to specified url
                var request = new HttpRequestMessage
                {
                    RequestUri = new Uri($"https://api.ksoft.si/images/random-image?tag={tag}"),
                    Method = HttpMethod.Get,
                    Headers =
                    {
                        { "Authorization", $"Bearer {Configuration.KSoftApiKey}" }
                    }
                };

                var jsonResponse = await HttpHelper.HttpRequestAndReturnJson(request);

                //Deserialize json response
                var image = JsonConvert.DeserializeObject<KSoftImages>(jsonResponse);

                var embed = new EmbedBuilder()
                    .WithImageUrl(image.Url)
                    .WithColor(Color.DarkBlue)
                    .WithFooter("Powered by KSoft.Si")
                    .Build();

                return embed;
            }
            catch (Exception e)
            {
                return CustomFormats.CreateErrorEmbed(e.Message);
            }
        }
    }
}