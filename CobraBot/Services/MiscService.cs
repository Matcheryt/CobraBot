using System;
using System.Net.Http;
using System.Threading.Tasks;
using CobraBot.Common;
using CobraBot.Handlers;
using CobraBot.Helpers;
using Discord;
using Newtonsoft.Json.Linq;

namespace CobraBot.Services
{
    public sealed class MiscService
    {
        /// <summary>Converts currency and returns the conversion.
        /// </summary>
        public static async Task<Embed> ConvertCurrencyAsync(string from, string to, string value)
        {
            if (value.Contains(","))
                return EmbedFormats.CreateErrorEmbed(
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
                return EmbedFormats.CreateErrorEmbed(e.Message);
            }
        }
    }
}
