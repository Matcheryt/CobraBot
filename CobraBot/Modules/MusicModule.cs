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
        public MusicService AudioService { get; set; }

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

            await Context.Message.AddReactionAsync(await AudioService.JoinAsync(Context.Guild, voiceState, Context.User as ITextChannel));
        }

        [Command("leave")]
        public async Task Leave()
            => await Context.Message.AddReactionAsync(await AudioService.LeaveAsync(Context.Guild));                   

        [Command("lyrics")]
        public async Task FetchLyrics()
            => await ReplyAsync(embed: await AudioService.FetchLyricsAsync(Context.Guild));

        [Command("play"), Alias("p")]
        public async Task Play([Remainder] string search)
            => await ReplyAsync(embed: await AudioService.PlayAsync(Context.User as SocketGuildUser, Context.Guild, Context, search));

        [Command("stop")]
        public async Task Stop()
            => await Context.Message.AddReactionAsync(await AudioService.StopAsync(Context.Guild));

        [Command("queue"), Alias("q")]
        public async Task Queue()
            => await ReplyAsync(embed: await AudioService.QueueAsync(Context.Guild));

        [Command("remove")]
        public async Task Remove(int index, int indexMax = 0)
            => await ReplyAsync(embed: await AudioService.RemoveFromQueueAsync(Context.Guild, index, indexMax));

        [Command("shuffle")]
        public async Task Shuffle()
            => await ReplyAsync(embed: await AudioService.ShuffleAsync(Context.Guild));

        [Command("skip")]
        public async Task Skip()
            => await ReplyAsync(embed: await AudioService.SkipTrackAsync(Context.Guild));

        //[Command("volume")]
        //public async Task Volume(int volume)
        //    => await ReplyAsync(await MusicService.SetVolumeAsync(Context.Guild, volume));

        [Command("pause")]
        public async Task Pause()
            => await ReplyAsync(embed: await AudioService.PauseAsync(Context.Guild));

        [Command("resume")]
        public async Task Resume()
            => await ReplyAsync(embed: await AudioService.ResumeAsync(Context.Guild));
    }
}