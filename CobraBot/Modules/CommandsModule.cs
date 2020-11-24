using Discord.Commands;
using Discord;
using System.Threading.Tasks;
using System;
using CobraBot.Helpers;
using CobraBot.Handlers;

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

        //Sets custom prefix
        [RequireUserPermission(GuildPermission.Administrator)]
        [Command("prefix")]
        public async Task SetPrefix(string prefix)
        {
            //If user input == default
            if (prefix == "default")
            {
                //Check if the guild has custom prefix
                string currentPrefix = DatabaseHandler.GetPrefix(Context.Guild.Id);

                //If the guild doesn't have custom prefix, return
                if (currentPrefix == null)
                {
                    await ReplyAsync(embed: await Helper.CreateErrorEmbed("Bot prefix is already the default one!"));
                    return;
                }

                //If they have a custom prefix, remove it from database and consequently setting it to default
                DatabaseHandler.RemovePrefixFromDB(Context.Guild.Id);
                await ReplyAsync(embed: await Helper.CreateBasicEmbed("", "Bot prefix was reset to:  **-**", Color.DarkGreen));
                return;
            }

            //If user input is longer than 5, return
            if (prefix.Length > 5)
            {
                await ReplyAsync(embed: await Helper.CreateErrorEmbed("Bot prefix can't be longer than 5 characters!"));
                return;
            }

            //If every check passes, we add the new custom prefix to the database
            DatabaseHandler.AddPrefixToDB(Context.Guild.Id, prefix);
            await ReplyAsync(embed: await Helper.CreateBasicEmbed("Prefix Changed", $"Bot's prefix is now:  **{prefix}**", Color.DarkGreen));
        }

        //Random number between minVal and maxVal
        [Command("random")]
        public async Task RandomNumber(int minVal = 0, int maxVal = 0)
        {
            #region ErrorHandling
            if (minVal > maxVal)
            {
                await ReplyAsync(embed: await Helper.CreateErrorEmbed("**Minimum Value cannot be greater than Maximum Value**"));
                return;
            }

            if (minVal >= 2147483647 || maxVal >= 2147483647)
            {
                await ReplyAsync(embed: await Helper.CreateErrorEmbed("**Value cannot be greater than 2147483647**"));
                return;
            }
            else if (minVal <= -2147483647 || maxVal <= -2147483647)
            {
                await ReplyAsync(embed: await Helper.CreateErrorEmbed("**Value cannot be lesser than -2147483647**"));
                return;
            }
            #endregion

            try
            {
                Random r = new Random();
                int randomNumber = r.Next(minVal, maxVal + 1);
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

        //Clean messages
        [RequireBotPermission(GuildPermission.ManageMessages)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [Command("clean", RunMode = RunMode.Async)]
        public async Task LimparMensagens(int count = 1)
        {
            //We only delete 100 messages at a time to prevent bot from getting overloaded
            if (count <= 100)
            {
                /* Saves all messages user specified in a variable, next
                   those messages are deleted and a message is sent to the textChannel
                   saying that X messages were deleted <- this message is deleted 2.3s later */
                try
                {
                    //Save messages to delete in a variable
                    var messagesToDelete = await Context.Channel.GetMessagesAsync(count + 1).FlattenAsync();
                    
                    //Delete messages to delete
                    await Context.Guild.GetTextChannel(Context.Channel.Id).DeleteMessagesAsync(messagesToDelete);
                    
                    //Message sent informing that X messages were deleted
                    var tempMessage = await Context.Channel.SendMessageAsync("Deleted " + "**" + count + "**" + " messages :white_check_mark:");            

                    //Delete temp message after 2.3s
                    await Task.Delay(2300);
                    await tempMessage.DeleteAsync();
                }
                catch(Exception)
                {
                    await ReplyAsync(embed: await Helper.CreateErrorEmbed("**Something went wrong!** Please try again"));
                }               
            }
            else
            {
                await ReplyAsync(embed: await Helper.CreateErrorEmbed(Context.User.Mention + " You cannot delete more than 100 messages at once"));
            }
            
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