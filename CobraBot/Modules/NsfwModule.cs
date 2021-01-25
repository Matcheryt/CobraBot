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

using System.Threading.Tasks;
using CobraBot.Preconditions;
using CobraBot.Services;
using Discord.Commands;

namespace CobraBot.Modules
{
    [RequireContext(ContextType.Guild)]
    [Name("NSFW"), RequireNsfw, Ratelimit(5, 2180, Measure.Milliseconds, RatelimitFlags.ApplyPerGuild)]
    public class NsfwModule : ModuleBase<SocketCommandContext>
    {
        [Command("nsfw")]
        [Name("NSFW"), Summary("Shows random NSFW image (real life stuff).")]
        public async Task Nsfw()
            => await NsfwService.GetRandomNsfwAsync(Context);


        [Command("nsfw gif")]
        [Name("NSFW gif"), Summary("Shows random NSFW gif (real life stuff).")]
        public async Task NsfwGif()
            => await NsfwService.GetRandomNsfwAsync(Context, true);


        [Command("nsfw subreddit")]
        [Name("NSFW Subreddit"), Summary("Shows a random post from specified NSFW subreddit. Span can be `hour`, `day`, `week`, `month`, `year` and `all`. Default: `day`")]
        public async Task NsfwSubreddit(string subreddit, string span = "day")
            => await ReplyAsync(embed: await NsfwService.GetRandomNsfwPostAsync(subreddit, span));


        [Command("hentai")]
        [Name("Hentai"), Summary("Shows a random hentai image.")]
        public async Task Hentai()
            => await NsfwService.GetNsfwImageFromTagAsync(Context,"hentai");


        [Command("hentai gif")]
        [Name("Hentai gif"), Summary("Shows a random hentai gif.")]
        public async Task HentaiGif()
            => await NsfwService.GetNsfwImageFromTagAsync(Context, "hentai_gif");


        [Command("neko")]
        [Name("Neko"), Summary("Shows a random neko image.")]
        public async Task Neko()
            => await NsfwService.GetNsfwImageFromTagAsync(Context, "neko");


        [Command("ass")]
        [Name("Ass"), Summary("Shows a random ass image.")]
        public async Task Ass()
            => await NsfwService.GetNsfwImageFromTagAsync(Context, "ass");
    }
}
