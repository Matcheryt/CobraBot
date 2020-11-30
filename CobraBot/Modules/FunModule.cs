using CobraBot.Helpers;
using Discord;
using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace CobraBot.Modules
{
    public class FunModule : ModuleBase<SocketCommandContext>
    {
        //Random number between minVal and maxVal
        [Command("random")]
        public async Task RandomNumber(int minVal = 0, int maxVal = 0)
        {
            #region ErrorHandling
            //If minVal > maxVal, Random.Next will throw an exception
            //So we switch minVal with maxVal and vice versa. That way we don't get an exception
            if (minVal > maxVal)
            {
                int tmp = minVal; //temporary variable to store minVal because it will be overwritten with maxVal
                minVal = maxVal;
                maxVal = tmp;
            }

            if (minVal > 2147483647 || maxVal > 2147483647)
            {
                await ReplyAsync(embed: await Helper.CreateErrorEmbed("**Value cannot be greater than 2147483647**"));
                return;
            }
            else if (minVal < -2147483647 || maxVal < -2147483647)
            {
                await ReplyAsync(embed: await Helper.CreateErrorEmbed("**Value cannot be lesser than -2147483647**"));
                return;
            }
            #endregion

            try
            {
                Random r = new Random();
                int randomNumber = r.Next(minVal, maxVal);
                await ReplyAsync(":game_die: Random number is: " + randomNumber);
            }
            catch (Exception)
            {
                await ReplyAsync(embed: await Helper.CreateErrorEmbed("**An error ocurred** Please check command syntax."));
            }
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
            var emojisToReact = new Emoji[2] { checkEmoji, wrongEmoji };

            await sentMessage.AddReactionsAsync(emojisToReact);
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
