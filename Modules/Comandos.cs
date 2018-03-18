using Discord.Commands;
using Discord;
using System.Threading.Tasks;
using System;
using Discord.WebSocket;

namespace CobraBot.Modules
{
    public class Comandos : ModuleBase<SocketCommandContext>
    {
        //Default error builder
        EmbedBuilder errorBuilder = new EmbedBuilder().WithColor(Color.Red);

        //Defines game bot is currently Playing
        [RequireOwner]
        [Command("setbotgame")]
        public async Task SetGame(string game)
        {
            if (game == null)
                return;

            if (!(Context.User.Id == 1234)) //Replace your discord user id where is '1234'
            {
                return;
            }
            else
            {
                await (Context.Client as DiscordSocketClient).SetGameAsync(game);
                Console.WriteLine($"{DateTime.Now}: Game was changed to {game}");
            }

        }

        //Random number between minVal and maxVal
        [Command("random")]
        public async Task RandomNumber(int minVal = 0, int maxVal = 0)
        {
            #region ErrorHandling
            if (minVal > maxVal)
            {
                errorBuilder.WithDescription("**Minimum Value cannot be greater than Maximum Value**");
                await ReplyAsync("", false, errorBuilder.Build());
                return;
            }

            if (minVal >= 2147483647 || maxVal >= 2147483647)
            {
                errorBuilder.WithDescription("**Value cannot be greater than 2147483647**");
                await ReplyAsync("", false, errorBuilder.Build());
                return;
            }
            else if (minVal <= -2147483647 || maxVal <= -2147483647)
            {
                errorBuilder.WithDescription("**Value cannot be lesser than -2147483647**");
                await ReplyAsync("", false, errorBuilder.Build());
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
                errorBuilder.WithDescription("**An error ocurred** Please check command syntax.");
            }         
        }

        //Shows help
        [Command("help")]
        public async Task Help()
        {
            EmbedBuilder builder = new EmbedBuilder();

            builder.WithTitle("Cobra Commands")
                .WithDescription(
                "**General**" +
                "\n-random (minNumber, maxNumber) - Gets a random number between minNumber and maxNumber" +
                "\n-help - Shows this help message" +
                "\n-usinfo (@User) - Shows info about the mentioned user" +
                "\n-clean (numberOfMessages) - Cleans messages from chat with specified number" +
                "\n-lmgtfy (text) - Creates a lmgtfy link about text inputed" +
                "\n-dict (word) - UNDER CONSTRUCTION" +
                "\n-fort (forniteUsername) - Shows fortnite profile info for a specific username" +
                "\n-steam (id) - Shows Steam profile Info for a specific SteamID" +
                "\n-weather (city) - Shows current weather for specific city\n\n" +
                "**Music**" +
                "\n-stream (online stream url) - Streams music from Online Stream" +
                "\n-play (youtubeUrl) - Plays video from Youtube URL" +
                "\n-stop - Stops audio streaming and makes bot leave channel")
                .WithColor(Color.Red)
                .WithThumbnailUrl(Context.Client.CurrentUser.GetAvatarUrl());

            await Context.User.SendMessageAsync("", false, builder.Build());
        }

        //Clean messages
        [RequireBotPermission(GuildPermission.ManageMessages)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [Command("clean", RunMode = RunMode.Async)]
        public async Task LimparMensagens(int count = 1)
        {
            if (count <= 100)
            {
                /* Saves all messages user specified in a variable, next
                 * those messages are deleted and a message is sent to the textChannel
                   saying that X messages were deleted <- this message is deleted 2.3s later */
                try
                {
                    var messagesToDelete = await Context.Channel.GetMessagesAsync(count + 1).FlattenAsync();
                    var textChannelId = Context.Channel.Id;
                    await Context.Guild.GetTextChannel(Context.Channel.Id).DeleteMessagesAsync(messagesToDelete);
                    var tempMessage = await Context.Channel.SendMessageAsync("Deleted " + "**" + count + "**" + " messages :white_check_mark:");
                    var lastMessageId = tempMessage.Id;

                    await Task.Delay(2300);
                    await tempMessage.DeleteAsync();
                }
                catch(Exception)
                {
                    errorBuilder.WithDescription("**Something went wrong!** Please try again");
                    await ReplyAsync("", false, errorBuilder.Build());
                }               
            }
            else
            {
                await ReplyAsync(Context.User.Mention + " You cannot delete more than 100 messages");
            }
            
        }

        //Show discord user info
        [Command("usinfo")]
        public async Task GetInfo(IGuildUser user = null)
        {
            if (user == null)
            {
                errorBuilder.WithDescription("**Please specify a user**");
                await ReplyAsync("", false, errorBuilder.Build());
            }
            else
            {
                var application = await Context.Client.GetApplicationInfoAsync();
                var thumbnailUrl = user.GetAvatarUrl();
                var date = $"{user.CreatedAt.Day}/{user.CreatedAt.Month}/{user.CreatedAt.Year}";
                var joinedAt = $"{user.JoinedAt.Value.Day}/{user.JoinedAt.Value.Month}/{user.JoinedAt.Value.Year}";


                var author = new EmbedAuthorBuilder()
                {
                    Name = user.Username,
                    IconUrl = thumbnailUrl,
                };

                var embed = new EmbedBuilder()
                {
                    Color = new Color(29, 140, 209),
                    Author = author
                };

                var us = user as SocketGuildUser;
                var username = us.Username;
                var discr = us.Discriminator;
                var id = us.Id;
                var dat = date;
                var stat = us.Status;
                var joinedServer = joinedAt;
                var game = us.Activity;
                var nick = us.Nickname;
                embed.Title = $"{username} Info";
                embed.Description = $"Username: **{username}**\n"
                    + $"Discriminator: **{discr}**\n"
                    + $"User ID: **{id}**\n"
                    + $"Nickname: **{nick}**\n"
                    + $"Created At: **{date}**\n"
                    + $"Current Status: **{stat}**\n"
                    + $"Joined Server At: **{joinedServer}**\n"
                    + $"Playing: **{game}**";
                embed.WithThumbnailUrl(thumbnailUrl);

                await ReplyAsync("", false, embed.Build());
            }
        }
       
    }
}