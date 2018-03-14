using Discord.Commands;
using Discord;
using System.Threading.Tasks;
using Discord.WebSocket;
using Discord.Audio;
using System.Net;
using System.IO;
using System.Linq;
using CobraBot.Services;
using System;

namespace CobraBot.Modules
{
    public class MusicModule : ModuleBase<SocketCommandContext>
    {

        /* Don't know if this is working fine, because of new Discord.NET version
         * also I haven't worked on this for a while because my bot is hosted on Ubuntu
         * and the services required for the music to function properly, only work on Windows
           Be sure you host your bot on Windows Server or just install Linux version of the services */

        //Music Service service
        private MusicService musicService;
        //Constructor
        public MusicModule(MusicService _musicService)
        {
            musicService = _musicService;
        }

        //Play command
        [Command("play", RunMode = RunMode.Async)]
        public async Task PlayCmd([Remainder]string url)
        {

            var channel = (Context.Message.Author as IGuildUser).VoiceChannel;

            //User needs role to be able to play music
            var user = Context.User as SocketGuildUser;
            var role = (user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == "DJ");

            //If user hasn't DJ role, replies with insufficient permissions
            if (!user.Roles.Contains(role))
            {
                await ReplyAsync(":no_entry: Insufficient Permission");
            }
            else
            {
                if (channel == null)
                {
                    await ReplyAsync(":no_entry_sign: You need to be in a voice channel!");
                }
                else
                {
                    if (url.ToLower().Contains("youtube.com"))
                    {
                        //Joins channel
                        IVoiceChannel vchannel = (Context.User as IVoiceState).VoiceChannel;
                        IAudioClient client = await vchannel.ConnectAsync();
                        musicService.audioDict.TryAdd(Context.Guild.Id, client);

                        EmbedBuilder builder = new EmbedBuilder();

                        string thumbnailYoutube = url.Substring(url.IndexOf("=") + 1);

                        //Send an embed with the information of the video requested
                        builder.WithTitle("**Video Requested**")
                            .WithDescription("Playing: " +  musicService.GetInfoFromYouTube(url).Result + "\n\n")
                            .WithColor(Color.Green)
                            .WithFooter("Requested By: " + user.Username)
                            .WithThumbnailUrl("http://img.youtube.com/vi/" + thumbnailYoutube + "/default.jpg");

                        await ReplyAsync("", false, builder.Build());

                        //Creates stream
                        var output = musicService.StreamYoutube(url).StandardOutput.BaseStream;
                        var stream = client.CreatePCMStream(AudioApplication.Music, 128 * 1024);
                        await output.CopyToAsync(stream);
                        await stream.FlushAsync().ConfigureAwait(false);

                        //await StopCmd(Context, vchannel);

                    }
                    else
                    {
                        await ReplyAsync("You need to specify a **Youtube URL**");
                    }
                }
            }
        }
        
        //Streams audio from an online radio based on URL
        [Command("stream", RunMode = RunMode.Async)]
        public async Task Cacadora([Remainder]string url)
        {
            if (url == null)
                return;

            var channel = (Context.Message.Author as IGuildUser).VoiceChannel;

            //User needs role to be able to play music
            var user = Context.User as SocketGuildUser;
            var role = (user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == "DJ");

            if (!user.Roles.Contains(role))
            {
                await ReplyAsync(":no_entry: Insufficient Permission");
            }
            else
            {
                if (channel == null)
                {
                    await ReplyAsync(":no_entry_sign: You need to be in a voice channel!");
                }
                else
                {
                    await ReplyAsync(":exclamation: Please note that if you paste the wrong URL, the bot will still join the channel but play no sound! Make sure you paste the right online stream url. :exclamation:");

                    IVoiceChannel vchannel = (Context.User as IVoiceState).VoiceChannel;
                    IAudioClient client = await channel.ConnectAsync();
                    musicService.audioDict.TryAdd(Context.Guild.Id, client);

                    //Creates a request based on online radio url
                    WebRequest request = WebRequest.Create(url);
                    WebResponse response = await request.GetResponseAsync();

                    using (Stream streamResponse = response.GetResponseStream())
                    {
                        var output = musicService.CreateStream(url).StandardOutput.BaseStream;
                        var stream = client.CreatePCMStream(AudioApplication.Music, 128 * 1024);
                        await output.CopyToAsync(stream);
                        streamResponse.CopyTo(stream);
                        stream.FlushAsync().Wait();
                    }
                }
            }
        }

        //Stop command
        [Command("stop", RunMode = RunMode.Async)]
        public async Task StopCommand()
        {
            var user = Context.User as SocketGuildUser;
            var role = (user as IGuildUser).Guild.Roles.FirstOrDefault(x => x.Name == "DJ");
            var channel = (Context.Message.Author as IGuildUser)?.VoiceChannel;

            if (channel == null)
            {
                await Context.Channel.SendMessageAsync(":no_entry_sign: You need to be in a voice channel!");
                return;
            }

            if (!user.Roles.Contains(role))
            {
                await Context.Channel.SendMessageAsync(":no_entry: Insufficient Permission");
                return;
            }

            await musicService.StopCmd(Context, channel);

        }

    }
}