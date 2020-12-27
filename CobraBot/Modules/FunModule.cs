using Discord.Commands;
using System.Threading.Tasks;
using CobraBot.Services;
using Discord;

namespace CobraBot.Modules
{
    [RequireContext(ContextType.Guild)]
    [Name("Fun Module")]
    public class FunModule : ModuleBase<SocketCommandContext>
    {
        public FunService FunService { get; set; }

        //Random number between minVal and maxVal
        [Command("random")]
        [Name("Random"), Summary("Prints random number between two specified numbers.")]
        public async Task RandomNumber(int minVal = 0, int maxVal = 0)
            => await ReplyAsync(embed: FunService.RandomNumberAsync(minVal, maxVal));


        //Poll command
        [Command("poll")]
        [Name("Poll"), Summary("Creates a poll with specified question and choices.")]
        public async Task Poll(string question, string choice1, string choice2)
            => await FunService.CreatePollAsync(question, choice1, choice2, Context);


        //Random meme command
        [Command("rmeme"), Alias("rm", "rmeme", "memes", "meme", "randmeme")]
        [Name("Random meme"), Summary("Shows a random meme.")]
        public async Task RandomMeme()
            => await ReplyAsync(embed: await FunService.GetRandomMemeAsync(((ITextChannel)Context.Channel).IsNsfw));


        //Random wikihow command
        [Command("rwikihow"), Alias("rw", "rwikihow", "rwiki", "randwikihow")]
        [Name("Random wikiHow"), Summary("Shows a random wikiHow post.")]
        public async Task RandomWikiHow()
            => await ReplyAsync(embed: await FunService.GetRandomWikiHowAsync(((ITextChannel)Context.Channel).IsNsfw));


        //Random cute image/gif command
        [Command("rcute"), Alias("rc", "rcute", "aww", "cute", "randcute")]
        [Name("Random cute"), Summary("Shows a random cute picture.")]
        public async Task RandomCute()
            => await ReplyAsync(embed: await FunService.GetRandomCuteAsync(((ITextChannel)Context.Channel).IsNsfw));
    }
}
