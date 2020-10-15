using Discord;
using Discord.Audio;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using CobraBot.Services;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace CobraBot.Services
{
    public class MusicService
    {
        /* Updated the MusicModule.cs and MusicService.cs, it now works okay
         * but there are some bugs that need to be fixed and also some code optimization needs to be done. */

        //TODO
        //Add music queue

        public readonly ConcurrentDictionary<ulong, IAudioClient> audioDict = new ConcurrentDictionary<ulong, IAudioClient>();
        public readonly ConcurrentDictionary<ulong, AudioOutStream> audioStreams = new ConcurrentDictionary<ulong, AudioOutStream>();

        //Check if user is alone in voice chat, if true then bot leaves channel
        public async Task CheckIfAlone(SocketUser user, SocketVoiceState stateOld, SocketVoiceState stateNew)
        {
            try
            {
                if (user.IsBot)
                    return;
                if (stateOld.VoiceChannel == null)
                    return;
                if (!stateOld.VoiceChannel.Users.Contains(((SocketGuildUser)user).Guild.CurrentUser)) //Compare the ids instead, also CurrentUser has a VoiceChannel property I think stateOld.VoiceChannel.Id == guild.CurrentUeser.VoiceChannel.Id could work
                    return;
                if (stateOld.VoiceChannel == (stateNew.VoiceChannel ?? null))
                    return;
                int users = 0;
                foreach (var u in stateOld.VoiceChannel.Users)
                {
                    if (!u.IsBot)
                    {
                        users++;
                    }
                }
                if (users < 1)
                {
                    var userG = (SocketGuildUser)user;
                    audioDict.TryGetValue(userG.Guild.Id, out IAudioClient aClient);
                    if (aClient == null)
                    {
                        return;
                    }
                    await aClient.StopAsync();
                    aClient.Dispose();
                    audioDict.TryRemove(userG.Guild.Id, out aClient);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

        }

        //Check if bot is already in the channel
        public bool CheckIfAlreadyJoined(SocketCommandContext context, IVoiceChannel _channel)
        {
            audioDict.TryGetValue(context.Guild.Id, out IAudioClient aClient);

            //If it isn't then return false (false, the bot isn't joined)
            if (aClient == null)
            {
                return false;
            }
            else
            {
                //If the bot is joined then return true (true, the bot is joined)
                return true;
            }
        }

        //Stop command
        public async Task StopCmd(SocketCommandContext context, IVoiceChannel _channel, AudioOutStream audioStream)
        {
            try
            {
                audioDict.TryGetValue(context.Guild.Id, out IAudioClient aClient);
                //If audio client == null
                if (aClient == null)
                {
                    //It means the bot isn't connected to any voice channel, return
                    await context.Channel.SendMessageAsync(":no_entry_sign: Bot is not connected to any Voice Channels");
                    return;
                }

                var channel = (context.Guild as SocketGuild).CurrentUser.VoiceChannel as IVoiceChannel;
                //If user is in the same channel as the bot, and if the bot is connected
                if (channel.Id == _channel.Id)
                {
                    /*It means the bot is connected to the voice channel, so we
                      Disconnect from the channel, stop the audio client, dispose it,
                      close and clear the audio stream, and remove both the audio client and audio stream from the dictionary*/

                    await channel.DisconnectAsync();
                    await aClient.StopAsync();
                    aClient.Dispose();
                    audioStream.Close();
                    audioStream.Clear();
                    audioDict.TryRemove(context.Guild.Id, out aClient);
                    audioStreams.TryRemove(context.Guild.Id, out audioStream);
                }
                //If the user isn't in the same channel as the bot, then send error message
                else
                {
                    await context.Channel.SendMessageAsync(":no_entry_sign: You must be in the same channel as me!");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        //Create stream based on path
        public Process CreateStream(string path)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var ffmpeg = new ProcessStartInfo
                {
                    FileName = "ffmpeg.exe",
                    Arguments = $"-i {path} -f s16le -ar 48000 -ac 2 pipe:1",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };
                return Process.Start(ffmpeg);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                var ffmpeg = new ProcessStartInfo
                {
                    FileName = "ffmpeg",
                    Arguments = $"-i {path} -f s16le -ar 48000 -ac 2 pipe:1",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };
                return Process.Start(ffmpeg);
            }
            else
            {
                return null;
            }        
        }

        //Stream Youtube based on string user inputted
        public Process StreamYoutube(string songName)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Process currentsong = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = $"/C youtube-dl.exe -f bestaudio -o - \"ytsearch1:{songName}\" --force-ipv4 | ffmpeg -i pipe:0 -ac 2 -f s16le -ar 48000 pipe:1",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };

                currentsong.Start();
                return currentsong;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Process currentsong = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "/bin/bash",
                        Arguments = $"-c \"youtube-dl -f bestaudio --no-continue -q -o - \"ytsearch1:{songName}\" --force-ipv4 | ffmpeg -loglevel quiet -i pipe:0 -ac 2 -f s16le -ar 48000 pipe:1\"",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };

                currentsong.Start();
                return currentsong;
            }
            else
            {
                return null;
            }                        
        }

        //Get info from youtube (Title, duration, etc)
        public async Task<Tuple<string, string>> GetInfoFromYouTube(string songName)
        {
            TaskCompletionSource<Tuple<string, string>> tcs = new TaskCompletionSource<Tuple<string, string>>();

            new Thread(() =>
            {
                string title;
                string duration;

                //youtube-dl.exe
                Process youtubedl;

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    //Get Video Title
                    ProcessStartInfo youtubedlGetTitle = new ProcessStartInfo()
                    {
                        FileName = "youtube-dl.exe",
                        Arguments = $"-s -e --get-duration \"ytsearch1:{songName}\" --force-ipv4",
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        UseShellExecute = false     //Linux?
                    };
                    youtubedl = Process.Start(youtubedlGetTitle);
                    youtubedl.WaitForExit();
                    //Read Title
                    string[] lines = youtubedl.StandardOutput.ReadToEnd().Split('\n');

                    if (lines.Length >= 2)
                    {
                        title = lines[0];
                        duration = lines[1];
                    }
                    else
                    {
                        title = "No Title found";
                        duration = "0";
                    }

                    tcs.SetResult(new Tuple<string, string>(title, duration + "m"));
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    //Get Video Title
                    ProcessStartInfo youtubedlGetTitle = new ProcessStartInfo()
                    {
                        FileName = "youtube-dl",
                        Arguments = $"-s -e --get-duration \"ytsearch1:{songName}\" --force-ipv4",
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        UseShellExecute = false     //Linux?
                    };
                    youtubedl = Process.Start(youtubedlGetTitle);
                    youtubedl.WaitForExit();
                    //Read Title
                    string[] lines = youtubedl.StandardOutput.ReadToEnd().Split('\n');

                    if (lines.Length >= 2)
                    {
                        title = lines[0];
                        duration = lines[1];
                    }
                    else
                    {
                        title = "No Title found";
                        duration = "0";
                    }

                    tcs.SetResult(new Tuple<string, string>(title, duration + "m"));
                }
                else
                {

                }
                
            }).Start();

            Tuple<string, string> result = await tcs.Task;
            if (result == null)
                throw new Exception("youtube-dl failed to receive title!");

            return result;
        }
    }
}