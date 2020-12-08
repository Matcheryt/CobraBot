using System;
using System.Net;
using System.Threading.Tasks;
using CobraBot.Handlers;
using CobraBot.Helpers;
using Discord;
using Newtonsoft.Json.Linq;

namespace CobraBot.Services
{
    public sealed class MiscService
    {
        /// <summary>Returns discord user info.
        /// </summary>
        public async Task<Embed> ShowUserInfoAsync(IGuildUser user)
        {
            if (user == null)
                return await Helper.CreateErrorEmbed("**Please specify a user**");

            var thumbnailUrl = user.GetAvatarUrl();
            var accountCreationDate = $"{user.CreatedAt.Day}/{user.CreatedAt.Month}/{user.CreatedAt.Year}";
            var joinedAt = $"{user.JoinedAt.Value.Day}/{user.JoinedAt.Value.Month}/{user.JoinedAt.Value.Year}";
            var username = user.Username;
            var discriminator = user.Discriminator;
            var id = user.Id;
            var status = user.Status;
            var game = user.Activity;

            var author = new EmbedAuthorBuilder()
            {
                Name = user.Username + " info",
                IconUrl = thumbnailUrl,
            };

            var usernameField = new EmbedFieldBuilder().WithName("Username").WithValue(username).WithIsInline(true);
            var discriminatorField = new EmbedFieldBuilder().WithName("Discriminator").WithValue(discriminator).WithIsInline(true);
            var userIdField = new EmbedFieldBuilder().WithName("User ID").WithValue(id).WithIsInline(true);
            var createdAtField = new EmbedFieldBuilder().WithName("Created At").WithValue(accountCreationDate).WithIsInline(true);
            var currentStatusField = new EmbedFieldBuilder().WithName("Current Status").WithValue(status).WithIsInline(true);
            var joinedAtField = new EmbedFieldBuilder().WithName("Joined Server At").WithValue(joinedAt).WithIsInline(true);
            var playingField = new EmbedFieldBuilder().WithName("Playing").WithValue(game).WithIsInline(true);

            var embed = new EmbedBuilder()
                .WithColor(Color.DarkGreen)
                .WithAuthor(author)
                .WithThumbnailUrl(thumbnailUrl)
                .WithFields(usernameField, discriminatorField, userIdField, currentStatusField, createdAtField, joinedAtField, playingField);

            return embed.Build();
        }

        /// <summary>Converts currency and returns the conversion.
        /// </summary>
        public async Task<Embed> ConvertCurrencyAsync(string from, string to, string value)
        {
            if (value.Contains(","))
                return await Helper.CreateErrorEmbed(
                    "Make sure you're using dots for decimal places instead of commas!");

            try
            {
                //Create request to specified url
                var request = (HttpWebRequest)WebRequest.Create($"https://api.ksoft.si/kumo/currency?from={from}&to={to}&value={value}");
                request.Headers["Authorization"] = $"Bearer {Configuration.KSoftApiKey}";
                request.Method = "GET";

                string httpResponse = await Helper.HttpRequestAndReturnJson(request);

                var jsonParsed = JObject.Parse(httpResponse);

                string convertedValuePretty = (string)jsonParsed["pretty"];

                var embed = new EmbedBuilder()
                    .WithTitle($"{value} {from.ToUpper()} is currently {convertedValuePretty}")
                    .WithColor(Color.DarkBlue)
                    .WithFooter($"Powered by KSoft.Si").Build();

                return embed;
            }
            catch (Exception e)
            {
                return await Helper.CreateErrorEmbed(e.Message);
            }
        }
    }
}
