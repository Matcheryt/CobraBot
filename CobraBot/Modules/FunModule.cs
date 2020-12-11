using CobraBot.Helpers;
using Discord;
using Discord.Commands;
using System;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using CobraBot.Handlers;
using CobraBot.Services;

namespace CobraBot.Modules
{
    public class FunModule : ModuleBase<SocketCommandContext>
    {
        public FunService FunService { get; set; }

        //Random number between minVal and maxVal
        [Command("random")]
        public async Task RandomNumber(int minVal = 0, int maxVal = 0)
            => await ReplyAsync(embed: await FunService.RandomNumberAsync(minVal, maxVal));


        //Poll command
        [Command("poll")]
        public async Task Poll(string question, string choice1, string choice2)
            => await FunService.CreatePollAsync(question, choice1, choice2, Context);


        //Random meme command
        [Command("randmeme"), Alias("rm", "rmeme", "memes", "meme")]
        public async Task RandomMeme()
            => await ReplyAsync(embed: await FunService.GetRandomMemeAsync());


        //Random wikihow command
        [Command("randwikihow"), Alias("rw", "rwikihow", "rwiki")]
        public async Task RandomWikiHow()
            => await ReplyAsync(embed: await FunService.GetRandomWikiHowAsync());


        //Random cute image/gif command
        [Command("randcute"), Alias("rc", "rcute", "aww", "cute")]
        public async Task RandomCute()
            => await ReplyAsync(embed: await FunService.GetRandomCuteAsync());
    }
}
