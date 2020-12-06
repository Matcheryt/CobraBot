using CobraBot.Helpers;
using Discord;
using Discord.Commands;
using System;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using CobraBot.Handlers;

namespace CobraBot.Modules
{
    public class FunModule : ModuleBase<SocketCommandContext>
    {
        //Random number between minVal and maxVal
        [Command("random")]
        public async Task RandomNumber(int minVal = 0, int maxVal = 0)
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
            await ReplyAsync(":game_die: Random number is: " + randomNumber);
        }

        //Poll command
        [Command("poll", RunMode = RunMode.Async)]
        public async Task Poll([Remainder] string question)
        {
            await Context.Message.DeleteAsync();

            var pollEmbed = new EmbedBuilder()
                .WithTitle(question)
                .WithDescription("React with  :white_check_mark:  for yes\nReact with  :x:  for no")
                .WithColor(Color.Teal)
                .WithFooter($"Poll created by: {Context.User}");

            var sentMessage = await ReplyAsync(embed: pollEmbed.Build());

            var checkEmoji = new Emoji("✅");
            var wrongEmoji = new Emoji("❌");
            var emojisToReact = new[] { checkEmoji, wrongEmoji };

            await sentMessage.AddReactionsAsync(emojisToReact);
        }

        [Command("randmeme", RunMode = RunMode.Async), Alias("rm", "rmeme", "memes")]
        public async Task RandomMeme()
        {
            try
            {
                //Create request to specified url
                var request = (HttpWebRequest)WebRequest.Create("https://api.ksoft.si/images/random-meme");
                request.Headers["Authorization"] = $"Bearer {Configuration.KSoftApiKey}";
                request.Method = "GET";

                string httpResponse = await Helper.HttpRequestAndReturnJson(request);

                var jsonParsed = JObject.Parse(httpResponse);

                string title = (string) jsonParsed["title"];
                string imageUrl = (string) jsonParsed["image_url"];
                string source = (string) jsonParsed["source"];
                string subreddit = (string) jsonParsed["subreddit"];
                string author = (string) jsonParsed["author"];

                var embed = new EmbedBuilder()
                    .WithTitle(title)
                    .WithImageUrl(imageUrl)
                    .WithColor(Color.DarkBlue)
                    .WithFooter($"{subreddit}  •  {author}  |  Powered by KSoft.Si")
                    .WithUrl(source).Build();

                await ReplyAsync(embed: embed);
            }
            catch (Exception e)
            {
                await ReplyAsync(embed: await Helper.CreateErrorEmbed(e.Message));
            }
        }

        [Command("randwikihow", RunMode = RunMode.Async), Alias("rw", "rwikihow", "rwiki")]
        public async Task RandomWikiHow()
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

                await ReplyAsync(embed: embed);
            }
            catch (Exception e)
            {
                await ReplyAsync(embed: await Helper.CreateErrorEmbed(e.Message));
            }
        }

        [Command("randaww", RunMode = RunMode.Async), Alias("ra", "raww", "aww")]
        public async Task RandomAww()
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

                await ReplyAsync(embed: embed);
            }
            catch (Exception e)
            {
                await ReplyAsync(embed: await Helper.CreateErrorEmbed(e.Message));
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
