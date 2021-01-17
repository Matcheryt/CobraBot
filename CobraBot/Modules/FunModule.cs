using CobraBot.Services;
using Discord;
using Discord.Commands;
using System.Threading.Tasks;
using CobraBot.Preconditions;

namespace CobraBot.Modules
{
    [RequireContext(ContextType.Guild)]
    [Name("Fun")]
    public class FunModule : ModuleBase<SocketCommandContext>
    {
        public FunService FunService { get; set; }

        //Random meme command
        [Command("meme"), Alias("rm", "rmeme", "memes", "randmeme"), Cooldown(1700)]
        [Name("Random meme"), Summary("Shows a random meme.")]
        public async Task RandomMeme()
            => await ReplyAsync(embed: await FunService.GetRandomMemeAsync(((ITextChannel)Context.Channel).IsNsfw));


        //Random wikihow command
        [Command("wikihow"), Alias("rw", "rwikihow", "rwiki", "randwikihow"), Cooldown(1700)]
        [Name("Random wikiHow"), Summary("Shows a random wikiHow post.")]
        public async Task RandomWikiHow()
            => await ReplyAsync(embed: await FunService.GetRandomWikiHowAsync(((ITextChannel)Context.Channel).IsNsfw));


        //Random cute image/gif command
        [Command("cute"), Alias("rc", "rcute", "aww", "randcute"), Cooldown(1700)]
        [Name("Random cute"), Summary("Shows a random cute picture.")]
        public async Task RandomCute()
            => await ReplyAsync(embed: await FunService.GetRandomCuteAsync(((ITextChannel)Context.Channel).IsNsfw));


        [Command("subreddit"), Alias("sr", "subr", "sreddit"), Cooldown(1700)]
        [Name("Subreddit"), Summary("Shows a random post from specified subreddit. Span can be `hour`, `day`, `week`, `month`, `year` and `all`. Default: `day`")]
        public async Task RandomSubredditPost(string subreddit, string span = "day")
            => await ReplyAsync(embed: await FunService.GetRandomPostAsync(subreddit, span));



        [Command("pepe"), Cooldown(1700)]
        [Name("Pepe"), Summary("Shows a random pepe image.")]
        public async Task Pepe()
            => await ReplyAsync(embed: await FunService.GetImageFromTagAsync("pepe"));


        [Command("doge"), Cooldown(1700)]
        [Name("Doge"), Summary("Shows a random doge image.")]
        public async Task Doge()
            => await ReplyAsync(embed: await FunService.GetImageFromTagAsync("doge"));


        [Command("kappa"), Cooldown(1700)]
        [Name("Kappa"), Summary("Shows a random kappa image.")]
        public async Task Kappa()
            => await ReplyAsync(embed: await FunService.GetImageFromTagAsync("kappa"));


        [Command("dab"), Cooldown(1700)]
        [Name("Dab"), Summary("Shows a random dab image.")]
        public async Task Dab()
            => await ReplyAsync(embed: await FunService.GetImageFromTagAsync("dab"));


        [Command("birb"), Cooldown(1700)]
        [Name("Birb"), Summary("Shows a random birb image.")]
        public async Task Birb()
            => await ReplyAsync(embed: await FunService.GetImageFromTagAsync("birb"));


        [Command("dog"), Cooldown(1700)]
        [Name("Dog"), Summary("Shows a random dog image.")]
        public async Task Dog()
            => await ReplyAsync(embed: await FunService.GetImageFromTagAsync("dog"));


        [Command("cat"), Cooldown(1700)]
        [Name("Cat"), Summary("Shows a random cat image.")]
        public async Task Cat()
            => await ReplyAsync(embed: await FunService.GetImageFromTagAsync("cat"));


        [Command("fox"), Cooldown(1700)]
        [Name("Fox"), Summary("Shows a random fox image.")]
        public async Task Fox()
            => await ReplyAsync(embed: await FunService.GetImageFromTagAsync("fox"));


        [Command("fbi"), Cooldown(1700)]
        [Name("Fbi"), Summary("Shows a random fbi image.")]
        public async Task Fbi()
            => await ReplyAsync(embed: await FunService.GetImageFromTagAsync("fbi"));


        [Command("kiss"), Cooldown(1700)]
        [Name("Kiss"), Summary("Shows a random kiss image.")]
        public async Task Kiss()
            => await ReplyAsync(embed: await FunService.GetImageFromTagAsync("kiss"));


        [Command("pat"), Cooldown(1700)]
        [Name("Pat"), Summary("Shows a random pat image.")]
        public async Task Pat()
            => await ReplyAsync(embed: await FunService.GetImageFromTagAsync("pat"));


        [Command("hug"), Cooldown(1700)]
        [Name("Hug"), Summary("Shows a random hug image.")]
        public async Task Hug()
            => await ReplyAsync(embed: await FunService.GetImageFromTagAsync("hug"));


        [Command("lick"), Cooldown(1700)]
        [Name("Lick"), Summary("Shows a random lick image.")]
        public async Task Lick()
            => await ReplyAsync(embed: await FunService.GetImageFromTagAsync("lick"));


        [Command("headrub"), Cooldown(1700)]
        [Name("Hearub"), Summary("Shows a random headrub image.")]
        public async Task Headrub()
            => await ReplyAsync(embed: await FunService.GetImageFromTagAsync("headrub"));


        [Command("clap"), Cooldown(1700)]
        [Name("Clap"), Summary("Shows a random clap image.")]
        public async Task Clap()
            => await ReplyAsync(embed: await FunService.GetImageFromTagAsync("clap"));


        [Command("tickle"), Cooldown(1700)]
        [Name("Tickle"), Summary("Shows a random tickle image.")]
        public async Task Tickle()
            => await ReplyAsync(embed: await FunService.GetImageFromTagAsync("tickle"));
    }
}
