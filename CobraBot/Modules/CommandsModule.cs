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
            
            
            
        //Shows help
        [Command("help")]
        public async Task Help()
        {
            var helpMessage = new EmbedBuilder();

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