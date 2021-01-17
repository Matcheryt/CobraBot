using System.Threading.Tasks;
using CobraBot.Preconditions;
using CobraBot.Services;
using Discord.Commands;

namespace CobraBot.Modules
{
    [RequireContext(ContextType.Guild)]
    [Name("NSFW"), RequireNsfw]
    public class NsfwModule : ModuleBase<SocketCommandContext>
    {
        public NsfwService NsfwService { get; set; }

        [Command("nsfw"), Cooldown(1700)]
        [Name("NSFW"), Summary("Shows random NSFW image (real life stuff).")]
        public async Task Nsfw()
            => await ReplyAsync(embed: await NsfwService.GetRandomNsfwAsync());


        [Command("nsfw gif"), Cooldown(1700)]
        [Name("NSFW gif"), Summary("Shows random NSFW gif (real life stuff).")]
        public async Task NsfwGif()
            => await ReplyAsync(embed: await NsfwService.GetRandomNsfwAsync(true));


        [Command("hentai"), Cooldown(1700)]
        [Name("Hentai"), Summary("Shows a random hentai image.")]
        public async Task Hentai()
            => await ReplyAsync(embed: await NsfwService.GetNsfwImageFromTagAsync("hentai"));


        [Command("hentai gif"), Cooldown(1700)]
        [Name("Hentai gif"), Summary("Shows a random hentai gif.")]
        public async Task HentaiGif()
            => await ReplyAsync(embed: await NsfwService.GetNsfwImageFromTagAsync("hentai_gif"));


        [Command("neko"), Cooldown(1700)]
        [Name("Neko"), Summary("Shows a random neko image.")]
        public async Task Neko()
            => await ReplyAsync(embed: await NsfwService.GetNsfwImageFromTagAsync("neko"));


        [Command("ass"), Cooldown(1700)]
        [Name("Ass"), Summary("Shows a random ass image.")]
        public async Task Ass()
            => await ReplyAsync(embed: await NsfwService.GetNsfwImageFromTagAsync("ass"));
    }
}
