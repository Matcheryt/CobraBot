/*
    Multi-purpose Discord Bot named Cobra
    Copyright (C) 2021 Telmo Duarte <contact@telmoduarte.me>

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License as published
    by the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Affero General Public License for more details.

    You should have received a copy of the GNU Affero General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>. 
*/

using CobraBot.Services;
using Discord.Commands;
using System.Threading.Tasks;
using CobraBot.Preconditions;

namespace CobraBot.Modules
{
    [RequireContext(ContextType.Guild)]
    [Name("Music"), IsMusicBeingUsed]
    public class MusicModule : ModuleBase<SocketCommandContext>
    {
        public MusicService MusicService { get; set; }

        [Command("join")]
        [Name("Join"), Summary("Makes bot join voice channel.")]
        public async Task Join()
            => await MusicService.JoinAsync(Context);


        [Command("leave")]
        [Name("Leave"), Summary("Makes bot leave voice channel.")]
        public async Task Leave()
            => await MusicService.LeaveAsync(Context);


        [Command("play"), Alias("p")]
        [Name("Play"), Summary("Plays specified song.")]
        public async Task Play([Remainder] string search)
            => await MusicService.PlayAsync(Context, search);


        //[Command("play file"), Alias("pf")]
        //[Name("Play file"), Summary("Plays sound from file attached to the message.")]
        //public async Task PlayFile()
        //    => await MusicService.PlayFileAsync(Context);


        [Command("stop")]
        [Name("Stop"), Summary("Stops music playback.")]
        public async Task Stop()
            => await MusicService.StopAsync(Context);


        [Command("queue"), Alias("q")]
        [Name("Queue"), Summary("Shows songs queue.")]
        public async Task Queue()
            => await MusicService.QueueAsync(Context);


        [Command("lyrics"), Ratelimit(1, 1, Measure.Seconds)]
        [Name("Lyrics"), Summary("Displays lyrics for current song.")]
        public async Task FetchLyrics()
            => await ReplyAsync(embed: await MusicService.FetchLyricsAsync(Context.Guild));


        [Command("nowplaying"), Alias("np")]
        [Name("Now playing"), Summary("Shows currently playing song.")]
        public async Task NowPlaying()
            => await ReplyAsync(embed: await MusicService.NowPlayingAsync(Context.Guild));


        [Command("remove")]
        [Name("Remove"), Summary("Removes specified song from queue.")]
        public async Task Remove(int index, int indexMax = 0)
            => await ReplyAsync(embed: MusicService.RemoveFromQueueAsync(Context.Guild, index, indexMax));


        [Command("shuffle")]
        [Name("Shuffle"), Summary("Shuffles queue.")]
        public async Task Shuffle()
            => await MusicService.ShuffleAsync(Context);


        [Command("skip"), Alias("s")]
        [Name("Skip"), Summary("Skips current song.")]
        public async Task Skip()
            => await MusicService.SkipTrackAsync(Context);


        [Command("seek")]
        [Name("Seek"), Summary("Seeks current track to specified position. Position to seek must be in `hh:mm:ss` format.")]
        public async Task Seek([Name("position to seek")] string positionToSeek)
            => await MusicService.SeekTrackAsync(Context, positionToSeek);


        [Command("restart")]
        [Name("Restart"), Summary("Restarts the track that is currently playing.")]
        public async Task RestartTrack()
            => await MusicService.RestartTrackAsync(Context);


        [Command("search"), Ratelimit(1, 2870, Measure.Milliseconds)]
        [Name("Search"), Summary("Searches youtube.")]
        public async Task Search([Name("search query")][Remainder] string searchString)
            => await MusicService.SearchAsync(searchString, Context);


        [Command("pause")]
        [Name("Pause"), Summary("Pauses music playback.")]
        public async Task Pause()
            => await MusicService.PauseAsync(Context);


        [Command("resume")]
        [Name("Resume"), Summary("Resumes music playback.")]
        public async Task Resume()
            => await MusicService.ResumeAsync(Context);
    }
}