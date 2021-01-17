using System;
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
                var jsonResponse = await Helper.HttpRequestAndReturnJson(request);

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

                var jsonResponse = await Helper.HttpRequestAndReturnJson(request);

                //Deserialize json response
                var image = JsonConvert.DeserializeObject<KSoftImages>(jsonResponse);

                var embed = new EmbedBuilder()
                    .WithImageUrl(image.Url)
                    .WithColor(Color.DarkBlue)
                    .WithFooter($"Powered by KSoft.Si")
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
