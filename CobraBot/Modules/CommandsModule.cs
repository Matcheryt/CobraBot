using Discord.Commands;
using Discord;
using System.Threading.Tasks;
using System;
using CobraBot.Helpers;

namespace CobraBot.Modules
{
    public class CommandsModule : ModuleBase<SocketCommandContext>
    {
        //Defines game bot is currently Playing
        [RequireOwner]
        [Command("setbotgame")]
        public async Task SetGame(string game)
        {
            await Context.Client.SetGameAsync(game);
            Console.WriteLine($"{DateTime.Now}: Game was changed to {game}");
        }     

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
            catch(Exception)
            {
                await ReplyAsync(embed: await Helper.CreateErrorEmbed("**An error ocurred** Please check command syntax."));
            }         
        }

        //Poll command
        [Command("poll", RunMode = RunMode.Async)]
        public async Task Poll([Remainder] string question)
        { 
            await Context.Message.DeleteAsync();

            var sentMessage = await ReplyAsync(embed: await Helper.CreateBasicEmbed(Context.User.Username + " asked: " + question, 
                "React with  :white_check_mark:  for yes\nReact with  :x:  for no", 
                Color.Teal));

            var checkEmoji = new Emoji("✅");
            var wrongEmoji = new Emoji("❌");
            var emojisToReact = new Emoji[2] { checkEmoji, wrongEmoji };

            await sentMessage.AddReactionsAsync(emojisToReact);
        }


        //Shows help
        [Command("help")]
        public async Task Help()
        {
            EmbedBuilder helpMessage = new EmbedBuilder();

            helpMessage.WithTitle("Cobra Commands")
                .WithDescription("You can check Cobra's commands [here](https://cobra.telmoduarte.me).")
                .WithColor(Color.DarkGreen);

            await ReplyAsync("", false, helpMessage.Build());
        }      

        //Show discord user info
        [Command("usinfo")]
        public async Task GetInfo(IGuildUser user = null)
        {
            if (user == null)
            {
                await ReplyAsync(embed: await Helper.CreateErrorEmbed("**Please specify a user**"));
            }
            else
            {
                var thumbnailUrl = user.GetAvatarUrl();
                var accountCreationDate = $"{user.CreatedAt.Day}/{user.CreatedAt.Month}/{user.CreatedAt.Year}";
                var joinedAt = $"{user.JoinedAt.Value.Day}/{user.JoinedAt.Value.Month}/{user.JoinedAt.Value.Year}";

                var author = new EmbedAuthorBuilder()
                {
                    Name = user.Username + " info",
                    IconUrl = thumbnailUrl,
                };

                var embed = new EmbedBuilder()
                {
                    Color = new Color(29, 140, 209),
                    Author = author
                };

                var username = user.Username;
                var discriminator = user.Discriminator;
                var id = user.Id;
                var status = user.Status;
                var game = user.Activity;                  

                embed.Description = $"**Username:** {username}\n"
                    + $"**Discriminator:** {discriminator}\n"
                    + $"**User ID:** {id}\n"
                    + $"**Created At:** {accountCreationDate}\n"
                    + $"**Current Status:** {status}\n"
                    + $"**Joined Server At:** {joinedAt}\n"
                    + $"**Playing:** {game}";
                embed.WithThumbnailUrl(thumbnailUrl);

                await ReplyAsync("", false, embed.Build());
            }
        }
    }
}