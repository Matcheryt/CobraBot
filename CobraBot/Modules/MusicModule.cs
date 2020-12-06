using Discord.Commands;
using Discord;
using System.Threading.Tasks;
using Discord.WebSocket;
using CobraBot.Services;
using CobraBot.Helpers;

namespace CobraBot.Modules
{
    [RequireContext(ContextType.Guild)]
    public class MusicModule : ModuleBase<SocketCommandContext>
    {
        public MusicService MusicService { get; set; }

        [Command("join")]
        public async Task Join()
        {
            var voiceState = Context.User as IVoiceState;

            //If user isn't connected to a voice channel
            if (voiceState.VoiceChannel == null)
            {
                await ReplyAsync(embed: await Helper.CreateErrorEmbed("You must be connected to a voice channel!"));
                return;
            }

            await Context.Message.AddReactionAsync(await MusicService.JoinAsync(Context.Guild, voiceState, Context.User as ITextChannel));
        }

        [Command("leave")]
        public async Task Leave()
            => await Context.Message.AddReactionAsync(await MusicService.LeaveAsync(Context.Guild));                   

        [Command("lyrics")]
        public async Task FetchLyrics()
            => await ReplyAsync(embed: await MusicService.FetchLyricsAsync(Context.Guild));

        [Command("play"), Alias("p")]
        public async Task Play([Remainder] string search)
            => await ReplyAsync(embed: await MusicService.PlayAsync(Context.User as SocketGuildUser, Context.Guild, Context, search));

        [Command("stop")]
        public async Task Stop()
            => await Context.Message.AddReactionAsync(await MusicService.StopAsync(Context.Guild));

        [Command("queue"), Alias("q")]
        public async Task Queue()
            => await ReplyAsync(embed: await MusicService.QueueAsync(Context.Guild));

        [Command("remove")]
        public async Task Remove(int index, int indexMax = 0)
            => await ReplyAsync(embed: await MusicService.RemoveFromQueueAsync(Context.Guild, index, indexMax));

        [Command("shuffle")]
        public async Task Shuffle()
            => await ReplyAsync(embed: await MusicService.ShuffleAsync(Context.Guild));

        [Command("skip"), Alias("s")]
        public async Task Skip()
            => await ReplyAsync(embed: await MusicService.SkipTrackAsync(Context.Guild));

        [Command("search")]
        public async Task Search([Remainder] string searchString)
            => await ReplyAsync(await MusicService.SearchAsync(Context.Guild, searchString));

        [Command("pause")]
        public async Task Pause()
            => await ReplyAsync(embed: await MusicService.PauseAsync(Context.Guild));

        [Command("resume")]
        public async Task Resume()
            => await ReplyAsync(embed: await MusicService.ResumeAsync(Context.Guild));
    }
}