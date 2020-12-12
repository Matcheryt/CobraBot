using Discord.Commands;
using System.Threading.Tasks;
using CobraBot.Services;

namespace CobraBot.Modules
{
    [RequireContext(ContextType.Guild)]
    public class MusicModule : ModuleBase<SocketCommandContext>
    {
        public MusicService MusicService { get; set; }

        [Command("join")]
        public async Task Join()
            => await MusicService.JoinAsync(Context);

        [Command("leave")]
        public async Task Leave()
            => await MusicService.LeaveAsync(Context);                   

        [Command("lyrics")]
        public async Task FetchLyrics()
            => await ReplyAsync(embed: await MusicService.FetchLyricsAsync(Context.Guild));

        [Command("play"), Alias("p")]
        public async Task Play([Remainder] string search)
            => await MusicService.PlayAsync(Context, search);

        [Command("stop")]
        public async Task Stop()
            => await MusicService.StopAsync(Context);

        [Command("queue"), Alias("q")]
        public async Task Queue()
            => await MusicService.QueueAsync(Context);

        [Command("remove")]
        public async Task Remove(int index, int indexMax = 0)
            => await ReplyAsync(embed: await MusicService.RemoveFromQueueAsync(Context.Guild, index, indexMax));

        [Command("shuffle")]
        public async Task Shuffle()
            => await MusicService.ShuffleAsync(Context);

        [Command("skip"), Alias("s")]
        public async Task Skip()
            => await ReplyAsync(embed: await MusicService.SkipTrackAsync(Context.Guild));

        [Command("search")]
        public async Task Search([Remainder] string searchString)
            => await MusicService.SearchAsync(searchString, Context);

        [Command("pause")]
        public async Task Pause()
            => await ReplyAsync(embed: await MusicService.PauseAsync(Context.Guild));

        [Command("resume")]
        public async Task Resume()
            => await ReplyAsync(embed: await MusicService.ResumeAsync(Context.Guild));
    }
}