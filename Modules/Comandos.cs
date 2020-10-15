﻿using Discord.Commands;
using Discord;
using System.Threading.Tasks;
using System;
using Discord.WebSocket;
using System.Configuration;
using System.Collections.Concurrent;
using System.Linq;

namespace CobraBot.Modules
{
    public class Comandos : ModuleBase<SocketCommandContext>
    {
        //Helper declaration
        Helpers.Helpers helper = new Helpers.Helpers();

        //Defines game bot is currently Playing
        [RequireOwner]
        [Command("setbotgame")]
        public async Task SetGame(string game)
        {
            await (Context.Client as DiscordSocketClient).SetGameAsync(game);
            Console.WriteLine($"{DateTime.Now}: Game was changed to {game}");

        }

        //Random number between minVal and maxVal
        [Command("random")]
        public async Task RandomNumber(int minVal = 0, int maxVal = 0)
        {
            #region ErrorHandling
            if (minVal > maxVal)
            {
                helper.errorBuilder.WithDescription("**Minimum Value cannot be greater than Maximum Value**");
                await ReplyAsync("", false, helper.errorBuilder.Build());
                return;
            }

            if (minVal >= 2147483647 || maxVal >= 2147483647)
            {
                helper.errorBuilder.WithDescription("**Value cannot be greater than 2147483647**");
                await ReplyAsync("", false, helper.errorBuilder.Build());
                return;
            }
            else if (minVal <= -2147483647 || maxVal <= -2147483647)
            {
                helper.errorBuilder.WithDescription("**Value cannot be lesser than -2147483647**");
                await ReplyAsync("", false, helper.errorBuilder.Build());
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
                helper.errorBuilder.WithDescription("**An error ocurred** Please check command syntax.");
            }         
        }


        //[Command("poll")]
        //public async Task Poll([Remainder] string question)
        //{
        //    if (0 != 0)
        //    {
        //        await ReplyAsync("You have already started a poll! Use **-pollr** to get results and end active poll");
        //        return;
        //    }

        //    await Context.Message.DeleteAsync();

        //    EmbedBuilder builder = new EmbedBuilder();

        //    builder.WithTitle(Context.User.Username + " asked: " + question)
        //        .WithDescription("React with  :white_check_mark:  for yes\nReach with  :x:  for no")
        //        .WithColor(Color.Teal);

        //    var sentMessage = await ReplyAsync("", false, builder.Build());
        //    var checkEmoji = new Emoji("✅");
        //    var wrongEmoji = new Emoji("❌");
        //    var emojisToReact = new Emoji[2] { checkEmoji, wrongEmoji };

        //    await sentMessage.AddReactionsAsync(emojisToReact);

        //    var users = (sentMessage.GetReactionUsersAsync(checkEmoji, 50));
        //    var check = await users.CountAsync();
        //    Console.WriteLine(check);
        //}

        //[Command("pollr")]
        //public async Task GetPollResults()
        //{
        //    if (0 == 0)
        //    {
        //        await ReplyAsync("You haven't started a poll!");
        //        return;
        //    }

        //    var messaa = await Context.Channel.GetMessageAsync(1);
        //    var a = messaa.Content;
        //    Console.WriteLine(a);

        //    var checkEmoji = new Emoji("✅");
        //    var wrongEmoji = new Emoji("❌");
        //    Console.WriteLine(messaa.GetReactionUsersAsync(checkEmoji, 50));
        //    Console.WriteLine(messaa.GetReactionUsersAsync(wrongEmoji, 50));

        //    await Context.Message.DeleteAsync();

        //    EmbedBuilder builder = new EmbedBuilder();
        //}


        //Shows help
        [Command("help")]
        public async Task Help()
        {
            EmbedBuilder builder = new EmbedBuilder();

            builder.WithTitle("Cobra Commands")
                .WithDescription(
                "**General**" +
                "\n-help - Shows this help message" +
                "\n-random (minNumber, maxNumber) - Gets a random number between minNumber and maxNumber" +
                "\n-usinfo (@User) - Shows info about the mentioned user" +
                "\n-clean (numberOfMessages) - Cleans messages from chat with specified number" +
                "\n-lmgtfy (text) - Creates a lmgtfy link about text inputed" +
                "\n-dict (word) - Returns the definition of the specified word" +
                "\n-steam (id) - Shows Steam profile Info for a specific SteamID" +
                "\n-covid (country) - Shows COVID19 data for specified country" +
                "\n-weather (city) - Shows current weather for specific city\n\n" +
                "**Music**" +
                "\n-stream (stream url) - Streams music from an Online Stream URL" +
                "\n-play (song name) - Plays audio from youtube related to song name specified" +
                "\n-stop - Stops audio streaming and makes bot leave channel")
                .WithColor(Color.DarkGreen)
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
                   those messages are deleted and a message is sent to the textChannel
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
                    helper.errorBuilder.WithDescription("**Something went wrong!** Please try again");
                    await ReplyAsync("", false, helper.errorBuilder.Build());
                }               
            }
            else
            {
                await ReplyAsync(Context.User.Mention + " You cannot delete more than 100 messages at once");
            }
            
        }

        //Show discord user info
        [Command("usinfo")]
        public async Task GetInfo(IGuildUser user = null)
        {
            if (user == null)
            {
                helper.errorBuilder.WithDescription("**Please specify a user**");
                await ReplyAsync("", false, helper.errorBuilder.Build());
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