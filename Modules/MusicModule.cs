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

        /* Updated the MusicModule.cs and MusicService.cs, it now works okay
         * but there are some bugs that need to be fixed and also some code optimization needs to be done. */

        //TODO
        //Add music queue

        //Music Service service
        private MusicService musicService;

        //Constructor
        public MusicModule(MusicService _musicService)
        {
            musicService = _musicService;
        }

        #region ReusableMethods
        public async Task GetInfoFromYtAndSendMessage(string songName, SocketGuildUser user)
        {
            //Format embed to send
            EmbedBuilder builder = new EmbedBuilder();
            builder.WithTitle("**Video Requested**")
                .WithDescription("Playing: " + musicService.GetInfoFromYouTube(songName).Result + "\n\n")
                .WithColor(Color.Red)
                .WithFooter("Requested By: " + user.Username);

            //Send embed to text channel
            await ReplyAsync("", false, builder.Build());
        }

        public async Task Stream(MusicService musicService, string songName, AudioOutStream stream)
        {
            try
            {
                //Creates stream
                var output = musicService.StreamYoutube(songName).StandardOutput.BaseStream;
                await output.CopyToAsync(stream); //<--------
                await stream.FlushAsync().ConfigureAwait(false);
            }
            catch
            {

            }
        }

        public async Task CheckIfAudioStreamExistsAndStream(string songName, AudioOutStream audioStream, IAudioClient client)
        {
            //If there isn't a valid audio stream
            if (audioStream == null)
            {
                //Create an audio stream, and add it to the dictionary so we can keep track
                audioStream = client.CreatePCMStream(AudioApplication.Music, 128 * 1024);
                musicService.audioStreams.TryAdd(Context.Guild.Id, audioStream);
                await Stream(musicService, songName, audioStream);
            }
            //If there is already a valid audio stream
            else
            {
                //We close and clear it first
                audioStream.Close();
                audioStream.Clear();
                //Remove it from the dictionary as well
                musicService.audioStreams.TryRemove(Context.Guild.Id, out audioStream);
                //Then we create a new one
                audioStream = client.CreatePCMStream(AudioApplication.Music, 128 * 1024);
                //And add it to the dictionary
                musicService.audioStreams.TryAdd(Context.Guild.Id, audioStream);
                await Stream(musicService, songName, audioStream);
            }
        }
        #endregion

        //Play command
        [Command("play", RunMode = RunMode.Async)]
        public async Task PlayCmd([Remainder] string songName)
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
                    if (songName == null)
                    {
                        await ReplyAsync("**Song name** must not be empty.");
                    }
                    else
                    {
                        //Voice channel reference
                        IVoiceChannel vchannel = (Context.User as IVoiceState).VoiceChannel;
                        musicService.audioDict.TryGetValue(Context.Guild.Id, out IAudioClient client);
                        musicService.audioStreams.TryGetValue(Context.Guild.Id, out AudioOutStream audioStream);

                        //If bot isn't on the channel
                        if (musicService.CheckIfAlreadyJoined(Context, vchannel) == false)
                        {
                            //We connect to the channel and save the audio client in the dictionary so we can keep track of it
                            client = await vchannel.ConnectAsync();
                            musicService.audioDict.TryAdd(Context.Guild.Id, client);
                        }

                        //And then proceed to play music
                        //Get info from MusicService's GetInfoFromYoutube() method, and then send that info to the text channel
                        await GetInfoFromYtAndSendMessage(songName, user);

                        //Checks if audio stream exists, and proceeds to stream audio
                        await CheckIfAudioStreamExistsAndStream(songName, audioStream, client);

                        //When current music finishes, search for more in queue
                        //If there isn't more music, then Stop 
                        //TODO

                        await StopCommand();
                    }
                }
            }
        }       

        //Streams audio from an online radio based on URL
        [Command("stream", RunMode = RunMode.Async)]
        public async Task Cacadora([Remainder] string url)
        {
            if (url == null)
                return;

            var channel = (Context.Message.Author as IGuildUser).VoiceChannel;

            //musicService.CheckIfAlreadyJoined(Context, channel);

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

                    //Voice channel reference
                    IVoiceChannel vchannel = (Context.User as IVoiceState).VoiceChannel;
                    musicService.audioDict.TryGetValue(Context.Guild.Id, out IAudioClient client);
                    musicService.audioStreams.TryGetValue(Context.Guild.Id, out AudioOutStream audioStream);

                    //If bot isn't on the channel
                    if (musicService.CheckIfAlreadyJoined(Context, vchannel) == false)
                    {
                        //We connect to the channel and save the audio client in the dictionary so we can keep track of it
                        client = await channel.ConnectAsync();
                        musicService.audioDict.TryAdd(Context.Guild.Id, client);

                        if (audioStream == null)
                        {
                            //Creates stream
                            audioStream = client.CreatePCMStream(AudioApplication.Music, 128 * 1024);
                            musicService.audioStreams.TryAdd(Context.Guild.Id, audioStream);
                            //Creates a request based on online radio url
                            WebRequest request = WebRequest.Create(url);
                            WebResponse response = await request.GetResponseAsync();

                            using (Stream streamResponse = response.GetResponseStream())
                            {
                                await Stream(musicService, url, audioStream);
                            }

                            await StopCommand();
                        }
                        else
                        {
                            audioStream.Close();
                            audioStream.Clear();
                            musicService.audioStreams.TryRemove(Context.Guild.Id, out audioStream);
                            audioStream = client.CreatePCMStream(AudioApplication.Music, 128 * 1024);
                            musicService.audioStreams.TryAdd(Context.Guild.Id, audioStream);
                            //Creates a request based on online radio url
                            WebRequest request = WebRequest.Create(url);
                            WebResponse response = await request.GetResponseAsync();

                            using (Stream streamResponse = response.GetResponseStream())
                            {
                                await Stream(musicService, url, audioStream);
                            }

                            await StopCommand();
                        }
                    }
                    //If the bot is already on the channel
                    else
                    {
                        if (audioStream == null)
                        {
                            //Creates stream
                            audioStream = client.CreatePCMStream(AudioApplication.Music, 128 * 1024);
                            musicService.audioStreams.TryAdd(Context.Guild.Id, audioStream);
                            //Creates a request based on online radio url
                            WebRequest request = WebRequest.Create(url);
                            WebResponse response = await request.GetResponseAsync();

                            using (Stream streamResponse = response.GetResponseStream())
                            {
                                await Stream(musicService, url, audioStream);
                            }

                            await StopCommand();
                        }
                        else
                        {
                            audioStream.Close();
                            audioStream.Clear();
                            musicService.audioStreams.TryRemove(Context.Guild.Id, out audioStream);
                            audioStream = client.CreatePCMStream(AudioApplication.Music, 128 * 1024);
                            musicService.audioStreams.TryAdd(Context.Guild.Id, audioStream);
                            //Creates a request based on online radio url
                            WebRequest request = WebRequest.Create(url);
                            WebResponse response = await request.GetResponseAsync();

                            using (Stream streamResponse = response.GetResponseStream())
                            {
                                await Stream(musicService, url, audioStream);
                            }

                            await StopCommand();
                        }
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

            musicService.audioStreams.TryGetValue(Context.Guild.Id, out AudioOutStream audioOutStream);
            await musicService.StopCmd(Context, channel, audioOutStream);
        }

    }
}