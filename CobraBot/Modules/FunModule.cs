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
using Discord;
using Discord.Commands;

namespace CobraBot.Modules
{
    [RequireContext(ContextType.Guild)]
    [Name("Fun")]
    [Ratelimit(5, 2180, Measure.Milliseconds, RatelimitFlags.ApplyPerGuild)]
    public class FunModule : ModuleBase<SocketCommandContext>
    {
        //Random meme command
        [Command("meme")]
        [Alias("rm", "rmeme", "memes", "randmeme")]
        [Name("Random meme")]
        [Summary("Shows a random meme.")]
        public async Task RandomMeme()
        {
            await ReplyAsync(embed: await FunService.GetRandomMemeAsync(((ITextChannel)Context.Channel).IsNsfw));
        }


        //Random wikihow command
        [Command("wikihow")]
        [Alias("rw", "rwikihow", "rwiki", "randwikihow")]
        [Name("Random wikiHow")]
        [Summary("Shows a random wikiHow post.")]
        public async Task RandomWikiHow()
        {
            await ReplyAsync(embed: await FunService.GetRandomWikiHowAsync(((ITextChannel)Context.Channel).IsNsfw));
        }


        //Random cute image/gif command
        [Command("cute")]
        [Alias("rc", "rcute", "aww", "randcute")]
        [Name("Random cute")]
        [Summary("Shows a random cute picture.")]
        public async Task RandomCute()
        {
            await ReplyAsync(embed: await FunService.GetRandomCuteAsync(((ITextChannel)Context.Channel).IsNsfw));
        }


        [Command("subreddit")]
        [Alias("sr", "subr", "sreddit")]
        [Name("Subreddit")]
        [Summary(
            "Shows a random post from specified SFW subreddit. Span can be `hour`, `day`, `week`, `month`, `year` and `all`. Default: `day`")]
        public async Task RandomSubredditPost(string subreddit, string span = "day")
        {
            await ReplyAsync(embed: await FunService.GetRandomPostAsync(subreddit, span));
        }


        [Command("pepe")]
        [Name("Pepe")]
        [Summary("Shows a random pepe image.")]
        public async Task Pepe()
        {
            await ReplyAsync(embed: await FunService.GetImageFromTagAsync("pepe"));
        }


        [Command("doge")]
        [Name("Doge")]
        [Summary("Shows a random doge image.")]
        public async Task Doge()
        {
            await ReplyAsync(embed: await FunService.GetImageFromTagAsync("doge"));
        }


        [Command("kappa")]
        [Name("Kappa")]
        [Summary("Shows a random kappa image.")]
        public async Task Kappa()
        {
            await ReplyAsync(embed: await FunService.GetImageFromTagAsync("kappa"));
        }


        [Command("dab")]
        [Name("Dab")]
        [Summary("Shows a random dab image.")]
        public async Task Dab()
        {
            await ReplyAsync(embed: await FunService.GetImageFromTagAsync("dab"));
        }


        [Command("birb")]
        [Name("Birb")]
        [Summary("Shows a random birb image.")]
        public async Task Birb()
        {
            await ReplyAsync(embed: await FunService.GetImageFromTagAsync("birb"));
        }


        [Command("dog")]
        [Name("Dog")]
        [Summary("Shows a random dog image.")]
        public async Task Dog()
        {
            await ReplyAsync(embed: await FunService.GetImageFromTagAsync("dog"));
        }


        [Command("cat")]
        [Name("Cat")]
        [Summary("Shows a random cat image.")]
        public async Task Cat()
        {
            await ReplyAsync(embed: await FunService.GetImageFromTagAsync("cat"));
        }


        [Command("fox")]
        [Name("Fox")]
        [Summary("Shows a random fox image.")]
        public async Task Fox()
        {
            await ReplyAsync(embed: await FunService.GetImageFromTagAsync("fox"));
        }


        [Command("fbi")]
        [Name("Fbi")]
        [Summary("Shows a random fbi image.")]
        public async Task Fbi()
        {
            await ReplyAsync(embed: await FunService.GetImageFromTagAsync("fbi"));
        }


        [Command("kiss")]
        [Name("Kiss")]
        [Summary("Shows a random kiss image.")]
        public async Task Kiss()
        {
            await ReplyAsync(embed: await FunService.GetImageFromTagAsync("kiss"));
        }


        [Command("pat")]
        [Name("Pat")]
        [Summary("Shows a random pat image.")]
        public async Task Pat()
        {
            await ReplyAsync(embed: await FunService.GetImageFromTagAsync("pat"));
        }


        [Command("hug")]
        [Name("Hug")]
        [Summary("Shows a random hug image.")]
        public async Task Hug()
        {
            await ReplyAsync(embed: await FunService.GetImageFromTagAsync("hug"));
        }


        [Command("lick")]
        [Name("Lick")]
        [Summary("Shows a random lick image.")]
        public async Task Lick()
        {
            await ReplyAsync(embed: await FunService.GetImageFromTagAsync("lick"));
        }


        [Command("headrub")]
        [Name("Headrub")]
        [Summary("Shows a random headrub image.")]
        public async Task Headrub()
        {
            await ReplyAsync(embed: await FunService.GetImageFromTagAsync("headrub"));
        }


        [Command("clap")]
        [Name("Clap")]
        [Summary("Shows a random clap image.")]
        public async Task Clap()
        {
            await ReplyAsync(embed: await FunService.GetImageFromTagAsync("clap"));
        }


        [Command("tickle")]
        [Name("Tickle")]
        [Summary("Shows a random tickle image.")]
        public async Task Tickle()
        {
            await ReplyAsync(embed: await FunService.GetImageFromTagAsync("tickle"));
        }
    }
}