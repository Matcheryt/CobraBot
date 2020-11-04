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
                .WithDescription(
                "**General**" +
                "\n-help - Shows this help message" +
                "\n-random (minimum number, maximum number) - Gets a random number between minNumber and maxNumber" +
                "\n-usinfo (@User) - Shows info about the mentioned user" +
                "\n-clean (number of messages) - Cleans messages from chat with specified number" +
                "\n-poll (question) - Asks a question on the chat" +
                "\n-lmgtfy (text) - Creates a lmgtfy link with text specified" +
                "\n-dict (word) - Returns the definition of the specified word" +
                "\n-steam (id) - Shows Steam profile Info for a specific SteamID" +
                "\n-covid (country) - Shows COVID19 data for specified country" +
                "\n-weather (city) - Shows current weather for specific city\n\n" +
                "**Music**" +
                "\n-play (song name) - Plays song specified" +
                "\n-pause - Pauses music playback" +
                "\n-resume - Resumes music playback" +
                "\n-stop - Stops audio stream and makes bot leave channel" +
                "\n-queue - Lists queued songs" +
                "\n-skip - Skips current song" +
                "\n-remove (queue index) OR (queue start index, queue end index) - Removes songs from queue at index, or removes songs from start index to end index" +
                "\n-shuffle - Shuffles queue" +
                "\n-lyrics - Shows lyrics for current song")
                .WithColor(Color.DarkGreen)
                .WithThumbnailUrl(Context.Client.CurrentUser.GetAvatarUrl());

            await Context.User.SendMessageAsync("", false, helpMessage.Build());
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