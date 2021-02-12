/*
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
using Discord.Commands;
using Newtonsoft.Json;

namespace CobraBot.Services
{
    public sealed class NsfwService
    {
        /// <summary>Retrieves a random post from specified subreddit.
        /// </summary>
        public static async Task<Embed> GetRandomNsfwPostAsync(string subreddit, string span = "week")
        {
            string[] availableSpans = { "hour", "day", "week", "month", "year", "all" };

            if (!availableSpans.Contains(span))
                return CustomFormats.CreateErrorEmbed(
                    $"Invalid span `{span}`. Span can be `hour`, `day`, `week`, `month`, `year` and `all`");

            try
            {
                //Create request to specified url
                var request = new HttpRequestMessage()
                {
                    RequestUri = new Uri($"https://api.ksoft.si/images/rand-reddit/{subreddit}?span={span}&remove_nsfw=false"),
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
                return CustomFormats.CreateErrorEmbed(httpException.StatusCode == HttpStatusCode.NotFound ? "**Subreddit not found**" : e.Message);
            }
        }

        /// <summary>Retrieves a random nsfw image or gif from KSoft.Si database according to the specified tag.
        /// </summary>
        public static async Task GetRandomNsfwAsync(SocketCommandContext context, bool gif = false)
        {
            //Create request to specified url
            var request = new HttpRequestMessage()
            {
                RequestUri =
                    gif
                        ? new Uri("https://api.ksoft.si/images/random-nsfw?gifs=true")
                        : new Uri("https://api.ksoft.si/images/random-nsfw"),
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
                var nsfw = JsonConvert.DeserializeObject<KSoftReddit>(jsonResponse);

                var embed = new EmbedBuilder()
                    .WithTitle($"{nsfw.Title}")
                    .WithImageUrl(nsfw.ImageUrl)
                    .WithColor(Color.DarkBlue)
                    .WithFooter($"{nsfw.Subreddit}  •  {nsfw.Author}  |  Powered by KSoft.Si")
                    .WithUrl(nsfw.Source).Build();

                await context.Channel.SendMessageAsync(embed: embed);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }


        /// <summary>Retrieves a random nsfw image from KSoft.Si database according to the specified tag.
        /// </summary>
        public static async Task GetNsfwImageFromTagAsync(SocketCommandContext context, string tag)
        {
            try
            {
                //Create request to specified url
                var request = new HttpRequestMessage()
                {
                    RequestUri = new Uri($"https://api.ksoft.si/images/random-image?tag={tag}&nsfw=true"),
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

                await context.Channel.SendMessageAsync(embed: embed);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
