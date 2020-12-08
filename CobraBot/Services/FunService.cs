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
    public sealed class FunService
    {
        /// <summary>Generates a random number.
        /// </summary>
        public async Task<Embed> RandomNumberAsync(int minVal, int maxVal)
        {
            //If minVal > maxVal, Random.Next will throw an exception
            //So we switch minVal with maxVal and vice versa. That way we don't get an exception
            if (minVal > maxVal)
            {
                int tmp = minVal; //temporary variable to store minVal because it will be overwritten with maxVal
                minVal = maxVal;
                maxVal = tmp;
            }

            var r = new Random();
            int randomNumber = r.Next(minVal, maxVal);
            return await Helper.CreateBasicEmbed("Random number", $":game_die: **{randomNumber}**", Color.DarkGreen);
        }

        /// <summary>Creates a poll with specified question and choices.
        /// </summary>
        public async Task CreatePollAsync(string question, string choice1, string choice2, SocketCommandContext context)
        {
            var pollEmbed = new EmbedBuilder()
                .WithTitle(question)
                .WithDescription($":one: {choice1}\n\n:two: {choice2}")
                .WithColor(Color.DarkGreen)
                .WithFooter($"Poll created by: {context.User}");

            var sentMessage = await context.Channel.SendMessageAsync(embed: pollEmbed.Build());

            var one = new Emoji("1️⃣");
            var two = new Emoji("2️⃣");
            await sentMessage.AddReactionsAsync(new[] { one, two });
        }

        /// <summary>Retrieves a random meme from KSoft.Si database.
        /// </summary>
        public async Task<Embed> GetRandomMemeAsync()
        {
            try
            {
                //Create request to specified url
                var request = (HttpWebRequest)WebRequest.Create("https://api.ksoft.si/images/random-meme");
                request.Headers["Authorization"] = $"Bearer {Configuration.KSoftApiKey}";
                request.Method = "GET";

                string httpResponse = await Helper.HttpRequestAndReturnJson(request);

                var jsonParsed = JObject.Parse(httpResponse);

                string title = (string)jsonParsed["title"];
                string imageUrl = (string)jsonParsed["image_url"];
                string source = (string)jsonParsed["source"];
                string subreddit = (string)jsonParsed["subreddit"];
                string author = (string)jsonParsed["author"];

                var embed = new EmbedBuilder()
                    .WithTitle(title)
                    .WithImageUrl(imageUrl)
                    .WithColor(Color.DarkBlue)
                    .WithFooter($"{subreddit}  •  {author}  |  Powered by KSoft.Si")
                    .WithUrl(source).Build();

                return embed;
            }
            catch (Exception e)
            {
                return await Helper.CreateErrorEmbed(e.Message);
            }
        }

        /// <summary>Retrieves a random WikiHow post from KSoft.Si database.
        /// </summary>
        public async Task<Embed> GetRandomWikiHowAsync()
        {
            try
            {
                //Create request to specified url
                var request = (HttpWebRequest)WebRequest.Create("https://api.ksoft.si/images/random-wikihow");
                request.Headers["Authorization"] = $"Bearer {Configuration.KSoftApiKey}";
                request.Method = "GET";

                string httpResponse = await Helper.HttpRequestAndReturnJson(request);

                var jsonParsed = JObject.Parse(httpResponse);

                string title = (string)jsonParsed["title"];
                string url = (string)jsonParsed["url"];
                string articleUrl = (string)jsonParsed["article_url"];

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
                return await Helper.CreateErrorEmbed(e.Message);
            }
        }

        /// <summary>Retrieves a random cute image/gif from KSoft.Si database.
        /// </summary>
        public async Task<Embed> GetRandomCuteAsync()
        {
            try
            {
                //Create request to specified url
                var request = (HttpWebRequest)WebRequest.Create("https://api.ksoft.si/images/random-aww");
                request.Headers["Authorization"] = $"Bearer {Configuration.KSoftApiKey}";
                request.Method = "GET";

                string httpResponse = await Helper.HttpRequestAndReturnJson(request);

                var jsonParsed = JObject.Parse(httpResponse);

                string title = (string)jsonParsed["title"];
                string imageUrl = (string)jsonParsed["image_url"];
                string source = (string)jsonParsed["source"];
                string subreddit = (string)jsonParsed["subreddit"];
                string author = (string)jsonParsed["author"];

                var embed = new EmbedBuilder()
                    .WithTitle(title)
                    .WithImageUrl(imageUrl)
                    .WithColor(Color.DarkBlue)
                    .WithFooter($"{subreddit}  •  {author}  |  Powered by KSoft.Si")
                    .WithUrl(source).Build();

                return embed;
            }
            catch (Exception e)
            {
                return await Helper.CreateErrorEmbed(e.Message);
            }
        }

        //[Command("pollshow")]
        //public async Task ShowPoll(ulong messageId)
        //{
        //    var message = await Context.Channel.GetMessageAsync(messageId);
        //    var reactions = message.Reactions;

        //    int answer1 = 0, answer2 = 0;

        //    foreach (IEmote emote in reactions.Keys)
        //    {
        //        if (emote.Name == ":white_check_mark:")
        //            answer1++;

        //        if (emote.Name == ":x:")
        //            answer2++;
        //    }

        //    answer1--;
        //    answer2--;

        //    await ReplyAsync($"{answer1} {answer2} \n {message.");
        //}
    }
}
