using CobraBot.Common.EmbedFormats;
using CobraBot.Handlers;
using CobraBot.Helpers;
using Discord;
using Newtonsoft.Json.Linq;
using System;
using System.Drawing;
using System.Net.Http;
using System.Threading.Tasks;
using Discord.Commands;
using Color = Discord.Color;

namespace CobraBot.Services
{
    public sealed class MiscService
    {
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
        public static Embed GetColorAsync(string hexColor)
        {
            System.Drawing.Color color;

            try
            {
                color = ColorTranslator.FromHtml(hexColor);
            }
            catch (Exception)
            {
                return CustomFormats.CreateErrorEmbed("**Color not found!**");
            }

            var colorEmbed = new EmbedBuilder()
                .WithColor(color.R, color.G, color.B)
                .WithDescription($"**RGB:** {color.R}, {color.G}, {color.B}");

            colorEmbed.WithTitle(color.Name);

            return colorEmbed.Build();
        }
    }
}