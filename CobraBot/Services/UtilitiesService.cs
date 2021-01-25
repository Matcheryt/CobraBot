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

using CobraBot.Common.EmbedFormats;
using CobraBot.Handlers;
using CobraBot.Helpers;
using Discord;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Discord.Commands;
using Color = Discord.Color;

namespace CobraBot.Services
{
    public sealed class UtilitiesService
    {
        /// <summary>Generates a random number.
        /// </summary>
        public static Embed RandomNumberAsync(int minVal, int maxVal)
        {
            //If minVal > maxVal, Random.Next will throw an exception
            //So we switch minVal with maxVal and vice versa. That way we don't get an exception
            if (minVal > maxVal)
            {
                int tmp = minVal; //temporary variable to store minVal because it will be overwritten with maxVal
                minVal = maxVal;
                maxVal = tmp;
            }

            var randomNumber = new Random().Next(minVal, maxVal);
            return CustomFormats.CreateBasicEmbed("Random number", $":game_die: **{randomNumber}**", 0x268618);
        }


        /// <summary>Creates a poll with specified question and choices.
        /// </summary>
        public static async Task CreatePollAsync(string question, string choice1, string choice2, SocketCommandContext context)
        {
            var pollEmbed = new EmbedBuilder()
                .WithTitle(question)
                .WithDescription($":one: {choice1}\n\n:two: {choice2}")
                .WithColor(0x268618)
                .WithFooter($"Poll created by: {context.User}");

            var sentMessage = await context.Channel.SendMessageAsync(embed: pollEmbed.Build());

            var one = new Emoji("1️⃣");
            var two = new Emoji("2️⃣");
            await sentMessage.AddReactionsAsync(new[] { one, two });
        }

        /// <summary> Converts currency and returns the conversion. </summary>
        public static async Task<Embed> ConvertCurrencyAsync(string from, string to, string value)
        {
            if (value.Contains(","))
                return CustomFormats.CreateErrorEmbed(
                    "Make sure you're using dots for decimal places instead of commas!");

            try
            {
                //Create request to specified url
                var request = new HttpRequestMessage()
                {
                    RequestUri = new Uri($"https://api.ksoft.si/kumo/currency?from={from}&to={to}&value={value}"),
                    Method = HttpMethod.Get,
                    Headers =
                    {
                        { "Authorization", $"Bearer {Configuration.KSoftApiKey}" }
                    }
                };

                var jsonParsed = JObject.Parse(await Helper.HttpRequestAndReturnJson(request));

                string convertedValuePretty = (string)jsonParsed["pretty"];

                var embed = new EmbedBuilder()
                    .WithTitle($"{value} {from.ToUpper()} is currently {convertedValuePretty}")
                    .WithColor(Color.DarkBlue)
                    .WithFooter($"Powered by KSoft.Si").Build();

                return embed;
            }
            catch (Exception e)
            {
                return CustomFormats.CreateErrorEmbed(e.Message);
            }
        }


        /// <summary> Generate a LMGTFY link. </summary>
        public static string Lmgtfy(string textToSearch)
        {
            if (textToSearch.Contains(" "))
                textToSearch = textToSearch.Replace(" ", "+");

            return $"https://lmgtfy.app/?q={textToSearch}";
        }


        /// <summary>Shows user's avatar and provides links for download in various sizes and formats. </summary>
        public static Embed GetAvatar(SocketCommandContext context)
        {
            //Save avatar urls with different formats
            var pngUrl = context.User.GetAvatarUrl(ImageFormat.Png);
            var jpegUrl = context.User.GetAvatarUrl(ImageFormat.Jpeg);
            var webpUrl = context.User.GetAvatarUrl(ImageFormat.WebP);
            
            //Save avatar urls with different sizes
            var size16 = context.User.GetAvatarUrl(ImageFormat.WebP, 16);
            var size32 = context.User.GetAvatarUrl(ImageFormat.WebP, 32);
            var size64 = context.User.GetAvatarUrl(ImageFormat.WebP, 64);
            var size128 = context.User.GetAvatarUrl(ImageFormat.WebP);
            var size256 = context.User.GetAvatarUrl(ImageFormat.WebP, 256);
            var size512 = context.User.GetAvatarUrl(ImageFormat.WebP, 512);
            var size1024 = context.User.GetAvatarUrl(ImageFormat.WebP, 1024);
            var size2048 = context.User.GetAvatarUrl(ImageFormat.WebP, 2048);

            //Create the embed to show to the user
            var avatarEmbed = new EmbedBuilder()
                .WithFooter(x =>
                {
                    x.IconUrl = size2048;
                    x.Text = context.User.ToString();
                })
                .WithThumbnailUrl(size2048)
                .WithColor(Color.Blue)
                .AddField(x =>
                {
                    x.Name = "Formats";
                    x.Value = $"[png]({pngUrl}) | [jpeg]({jpegUrl}) | [webp]({webpUrl})";
                })
                .AddField(x =>
                {
                    x.Name = "Sizes";
                    x.Value =
                        $"[16]({size16}) | [32]({size32}) | [64]({size64}) | [128]({size128}) | [256]({size256}) | [512]({size512}) | [1024]({size1024}) | [2048]({size2048})";
                });

            return avatarEmbed.Build();
        }


        /// <summary> Shows hex color. </summary>
        public static async Task<Embed> GetRgbColor(string hexColor)
        {
            if (hexColor.Contains("#"))
                hexColor = hexColor.Replace("#", "");

            try
            {
                var response =
                    await Helper.HttpClient.GetAsync($"https://some-random-api.ml/canvas/rgb?hex={hexColor}");
                var jsonString = await response.Content.ReadAsStringAsync();

                var jsonParsed = JObject.Parse(jsonString);

                var r = (int)jsonParsed["r"];
                var g = (int)jsonParsed["g"];
                var b = (int)jsonParsed["b"];

                var imageUrl = $"https://some-random-api.ml/canvas/colorviewer?hex={hexColor}";
                return CustomFormats.CreateColorEmbed(imageUrl, r, g, b, hexColor);
            }
            catch (Exception)
            {
                return CustomFormats.CreateErrorEmbed("**Color not found!**");
            }
        }


        /// <summary> Shows hex color. </summary>
        public static async Task<Embed> GetHexColorAsync(int r, int g, int b)
        {
            try
            {
                var response =
                    await Helper.HttpClient.GetAsync($"https://some-random-api.ml/canvas/hex?rgb={r},{g},{b}");
                var jsonString = await response.Content.ReadAsStringAsync();

                var jsonParsed = JObject.Parse(jsonString);

                var hex = (string)jsonParsed["hex"];

                hex = hex?.Replace("#", "");

                var imageUrl = $"https://some-random-api.ml/canvas/colorviewer?hex={hex}";
                return CustomFormats.CreateColorEmbed(imageUrl, r, g, b, hex);
            }
            catch (Exception)
            {
                return CustomFormats.CreateErrorEmbed("**Color not found!**");
            }
        }
    }
}