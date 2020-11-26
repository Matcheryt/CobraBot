using Discord;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Victoria;
using Victoria.EventArgs;
using Victoria.Enums;
using Discord.Commands;
using CobraBot.Helpers;

namespace CobraBot.Services
{
    public sealed class MusicService
    {
        public readonly LavaNode _lavaNode;

        public MusicService(LavaNode lavaNode)
        {
            _lavaNode = lavaNode;
            _lavaNode.OnTrackEnded += OnTrackEnded;

        }

        /// <summary>Fired whenever someone joins/leaves a voice channel.
        /// <para>Used to automatically disconnect bot if bot is left alone in voice channel</para>
        /// </summary>
        public async Task UserVoiceStateUpdated(SocketUser user, SocketVoiceState oldState, SocketVoiceState newState)
        {
            if (!_lavaNode.HasPlayer(((SocketGuildUser)user).Guild))
                return;
            if (user.IsBot)
                return;
            if (oldState.VoiceChannel == null)
                return;
            if (!oldState.VoiceChannel.Users.Contains(((SocketGuildUser)user).Guild.CurrentUser))
                return;
            if (oldState.VoiceChannel == (newState.VoiceChannel ?? null))
                return;

            int users = 0;
            foreach (var u in oldState.VoiceChannel.Users)
            {
                if (!u.IsBot)
                {
                    users++;
                }
            }

            if (users < 1)
            {
                var player = _lavaNode.GetPlayer(((SocketGuildUser)user).Guild);
                await player.StopAsync();                
                await _lavaNode.LeaveAsync(player.VoiceChannel);
            }
        }

        /// <summary>Joins the voice channel the user is in and reacts to user message with 'okEmoji'.
        /// </summary>
        public async Task<Emoji> JoinAsync(IGuild guild, IVoiceState voiceState, ITextChannel textChannel)
        {
            var okEmoji = new Emoji("👍");

            //If bot is already connected to a voice channel
            if (_lavaNode.HasPlayer(guild))
            {
                //Just return an okEmoji, as the bot is already connected
                return okEmoji;
            }

            //If bot isn't connected, then try to join
            try
            {
                await _lavaNode.JoinAsync(voiceState.VoiceChannel, textChannel);
                return okEmoji;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return okEmoji;
            }
        }

        /// <summary>Plays the requested song or adds it to the queue.
        /// <para>It also joins the voice channel if the bot isn't already joined.</para>
        /// </summary>
        public async Task<Embed> PlayAsync(SocketGuildUser user, IGuild guild, SocketCommandContext context, string query)
        {
            //Check If User Is Connected To Voice Cahnnel.
            if (user.VoiceChannel == null)
                return await Helper.CreateErrorEmbed("You must be connected to a voice channel!");

            //Check the guild has a player available.
            if (!_lavaNode.HasPlayer(guild))
            {
                var voiceState = context.User as IVoiceState;
                var textChannel = context.Channel as ITextChannel;
                await _lavaNode.JoinAsync(voiceState.VoiceChannel, textChannel);
            }

            try
            {
                //Get the player for that guild.
                var player = _lavaNode.GetPlayer(guild);
                
                //Find The Youtube Track the User requested.
                LavaTrack track = null;

                var search = Uri.IsWellFormedUriString(query, UriKind.Absolute) ?
                    await _lavaNode.SearchAsync(query)
                    : await _lavaNode.SearchYouTubeAsync(query);               

                //If we couldn't find anything, tell the user.
                if (search.LoadStatus == LoadStatus.NoMatches)
                    return await Helper.CreateErrorEmbed($"I wasn't able to find anything for {query}.");

                //If results derive from search results (ex: ytsearch: some song)
                if (search.LoadStatus == LoadStatus.SearchResult)
                {
                    //Then load the first track of the search results
                    track = search.Tracks.FirstOrDefault();

                    //If the Bot is already playing music, or if it is paused but still has music in the playlist, Add the requested track to the queue.
                    if (player.Track != null && (player.PlayerState is PlayerState.Playing || player.PlayerState is PlayerState.Paused))
                    {
                        player.Queue.Enqueue(track);
                        return await Helper.CreateBasicEmbed("", $"**{track.Title}** has been added to queue. [{context.User.Mention}]", Color.Blue);
                    }

                    //Player was not playing anything, so lets play the requested track.
                    await player.PlayAsync(track);

                    return await Helper.CreateBasicEmbed("Now playing", $"[{track.Title}]({track.Url})", Color.Blue);
                }
                //If results derive from a playlist
                else
                {
                    //If the Bot is already playing music, or if it is paused but still has music in the playlist
                    if (player.PlayerState is PlayerState.Playing && player.Track != null || player.PlayerState is PlayerState.Paused)
                    {
                        //Then add all the playlist songs to the queue
                        for (int i = 0; i < search.Tracks.Count; i++)
                        {
                            track = search.Tracks.ElementAt(i);
                            player.Queue.Enqueue(track);
                        }

                        //And send a message saying that X tracks have been added to queue
                        return await Helper.CreateBasicEmbed("", $"**{search.Tracks.Count} tracks** have been added to queue. [{context.User.Mention}]", Color.Blue);
                    }
                    //If the player isn't playing anything
                    else
                    {
                        //Then add all the songs EXCLUDING the first one, because we will play that one next
                        for (int i = 1; i < search.Tracks.Count; i++)
                        {
                            track = search.Tracks.ElementAt(i);
                            player.Queue.Enqueue(track);
                        }

                        //After adding every song except the first, we retrieve the first track
                        track = search.Tracks.FirstOrDefault();
                        //And ask the player to play it
                        await player.PlayAsync(track);

                        //Send a message saying that we are now playing the first track, and that X other tracks have been added to queue
                        return await Helper.CreateBasicEmbed("Now playing", $"[{track.Title}]({track.Url})\n{search.Tracks.Count-1} other tracks have been added to queue.", Color.Blue);
                    }
                }               
            }
            //If after all the checks we did, something still goes wrong. Tell the user about it so they can report it back to us.
            catch (Exception ex)
            {
                return await Helper.CreateErrorEmbed(ex.Message);
            }
        }

        /// <summary>Makes bot leave voice channel and reacts to user message with 'byeEmoji'.
        /// </summary>
        public async Task<Emoji> LeaveAsync(IGuild guild)
        {
            var byeEmoji = new Emoji("👋");

            if (!_lavaNode.HasPlayer(guild))
                return byeEmoji;

            try
            {
                //Get The Player Via GuildID.
                var player = _lavaNode.GetPlayer(guild);

                //if The Player is playing, Stop it.
                if (player.PlayerState is PlayerState.Playing)
                {
                    await player.StopAsync();
                }

                //Leave the voice channel.
                await _lavaNode.LeaveAsync(player.VoiceChannel);
               
                return byeEmoji;
            }
            //Tell the user about the error so they can report it back to us.
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return byeEmoji;
            }
        }

        /// <summary>Shuffles queue and returns an embed.
        /// </summary>
        public async Task<Embed> ShuffleAsync(IGuild guild)
        {
            //Checks if bot is connected to a voice channel
            if (!_lavaNode.HasPlayer(guild))
                return await Helper.CreateErrorEmbed("Player doesn't seem to be playing anything right now.");

            //Bot is connected to voice channel, so we get the player associated with the guild
            var player = _lavaNode.GetPlayer(guild);

            //Check if the queue is > 1
            if (player.Queue.Count <= 1)
                return await Helper.CreateErrorEmbed("No songs in queue to shuffle!");

            //If all conditions check, then shuffle the queue and send a message to the text channel
            player.Queue.Shuffle();
            return await Helper.CreateBasicEmbed("", "Queue shuffled.", Color.Blue);
        }

        /// <summary>Returns an embed containing the player queue.
        /// </summary>
        public async Task<Embed> QueueAsync(IGuild guild)
        {
            try
            {
                /* Create a string builder we can use to format how we want our list to be displayed. */
                var descriptionBuilder = new StringBuilder();

                /* Get The Player and make sure it isn't null. */
                if (!_lavaNode.HasPlayer(guild))
                    return await Helper.CreateErrorEmbed("Could not aquire player.");

                var player = _lavaNode.GetPlayer(guild);
                
                if (player.PlayerState is PlayerState.Playing)
                {
                    /*If the queue count is less than 1 and the current track IS NOT null then we wont have a list to reply with.
                        In this situation we simply return an embed that displays the current track instead. */
                    if (player.Queue.Count < 1 && player.Track != null)
                    {
                        return await Helper.CreateBasicEmbed($"", $"**Now playing: {player.Track.Title}**\nNo more songs queued.", Color.Blue);
                    }
                    else
                    {
                        /* Now we know if we have something in the queue worth replying with, so we itterate through all the Tracks in the queue.
                         *  Next Add the Track title and the url however make use of Discords Markdown feature to display everything neatly.
                            This trackNum variable is used to display the number in which the song is in place. (Start at 2 because we're including the current song.*/
                        var trackNum = 2;
                        foreach (LavaTrack track in player.Queue)
                        {
                            descriptionBuilder.Append($"{trackNum}: [{track.Title}]({track.Url}) \n");
                            trackNum++;
                        }
                        return await Helper.CreateBasicEmbed("Queue", $"Now Playing: [{player.Track.Title}]({player.Track.Url}) \n{descriptionBuilder}", Color.Blue);
                    }
                }
                else
                {
                    return await Helper.CreateErrorEmbed("Player doesn't seem to be playing anything right now.");
                }
            }
            catch (Exception ex)
            {
                return await Helper.CreateErrorEmbed(ex.Message);
            }

        }

        /// <summary>Removes specified track from queue and returns an embed.
        /// </summary>
        public async Task<Embed> RemoveFromQueueAsync(IGuild guild, int index, int indexMax)
        {           
            if (!_lavaNode.HasPlayer(guild))
                return await Helper.CreateErrorEmbed("Could not aquire player.");

            var player = _lavaNode.GetPlayer(guild);

            try 
            {
                //We decrement 2 to the index, as queue command shows first song in queue with number 2
                //and first item in queue has an index of 0
                index -= 2;

                /*By default indexMax = 0, so the user has the option to use the command with only 'index' which in turn removes
                  only 1 song from the queue. If the users chooses to use indexMax as well, then the bot knows that the user
                  wants to remove a range of songs instead of only 1 song. */
                if (indexMax != 0)
                {
                    //We decrement 2 to the index, as queue command shows first song in queue with number 2
                    //and first item in queue has an index of 0
                    indexMax -= 2;

                    int count = indexMax - index;

                    /*We use count+1 because RemoveRange() also counts the first index, for example:
                      If user wants to remove tracks number 2 to 5, it would only remove tracks 2, 3 and 4
                      because count would be = to 3 */
                    var tracksToRemove = player.Queue.RemoveRange(index, count+1);

                    return await Helper.CreateBasicEmbed("", $"Removed {tracksToRemove.Count} songs from queue", Color.Blue);
                }

                var trackToRemove = player.Queue.RemoveAt(index);

                return await Helper.CreateBasicEmbed("", $"Removed {trackToRemove.Title} from queue", Color.Blue);
            }
            catch (Exception ex)
            {
                return await Helper.CreateErrorEmbed(ex.Message);
            }
            
        }

        /// <summary>Skips current track and returns an embed.
        /// </summary>
        public async Task<Embed> SkipTrackAsync(IGuild guild)
        {
            try
            {
                var player = _lavaNode.GetPlayer(guild);

                /* Check if the player exists */
                if (player == null)
                    return await Helper.CreateErrorEmbed($"Could not aquire player.\nAre you using the bot right now? check -help for info on how to use the bot.");
                
                /* Check The queue, if it is less than one (meaning we only have the current song available to skip) it wont allow the user to skip.
                     User is expected to use the Stop command if they're only wanting to skip the current song. */
                if (player.Queue.Count < 1)
                {
                    return await Helper.CreateErrorEmbed($"Unable to skip a track as there is only one or no songs currently playing." +
                        $"\n\nDid you mean -stop?");
                }
                else
                {
                    try
                    {
                        /* Save the current song for use after we skip it. */
                        var currentTrack = player.Track;

                        /* Skip the current song. */
                        var nextTrack = await player.SkipAsync();

                        return await Helper.CreateBasicEmbed($"{currentTrack.Title} skipped.", $"Now playing: {nextTrack.Title}", Color.Blue);
                    }
                    catch (Exception ex)
                    {
                        return await Helper.CreateErrorEmbed(ex.Message);
                    }

                }
            }
            catch (Exception ex)
            {
                return await Helper.CreateErrorEmbed(ex.Message);
            }
        }

        /// <summary>Stops playback, clears queue, makes bot leave channel, and reacts to user message with 'stopEmoji'.
        /// </summary>
        public async Task<Emoji> StopAsync(IGuild guild)
        {
            var stopEmoji = new Emoji("🛑");

            try
            {
                if (!_lavaNode.HasPlayer(guild))
                    return stopEmoji;
                
                var player = _lavaNode.GetPlayer(guild);

                //If the player is playing, stop its playback
                if (player.PlayerState is PlayerState.Playing)
                {
                    await player.StopAsync();
                }

                //Clears the queue and makes bot leavhe channel
                player.Queue.Clear();
                await _lavaNode.LeaveAsync(player.VoiceChannel);

                return stopEmoji;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return stopEmoji;
            }
        }

        /*This is ran when a user uses the command Volume 
            Task Returns a String which is used in the command call. */
        //public async Task<string> SetVolumeAsync(IGuild guild, int volume)
        //{
        //    if (volume > 150 || volume <= 0)
        //    {
        //        return $"Volume must be between 1 and 150.";
        //    }
        //    try
        //    {
        //        var player = _lavaNode.GetPlayer(guild);
        //        await player.UpdateVolumeAsync((ushort)volume);
        //        return $"Volume has been set to {volume}.";
        //    }
        //    catch (InvalidOperationException ex)
        //    {
        //        return ex.Message;
        //    }
        //}

        /// <summary>Pauses current track and returns an embed.
        /// </summary>
        public async Task<Embed> PauseAsync(IGuild guild)
        {
            try
            {
                if (!_lavaNode.HasPlayer(guild))
                    return await Helper.CreateErrorEmbed("There is no music playing!");

                var player = _lavaNode.GetPlayer(guild);
                if (!(player.PlayerState is PlayerState.Playing))
                {
                    await player.PauseAsync();
                    return await Helper.CreateErrorEmbed("There is nothing to pause.");
                }

                await player.PauseAsync();
                return await Helper.CreateBasicEmbed("", $"**{player.Track.Title}** has been paused.", Color.Blue);
            }
            catch (InvalidOperationException ex)
            {
                return await Helper.CreateErrorEmbed(ex.Message);
            }
        }

        /// <summary>Resumes current track and returns an embed.
        /// </summary>
        public async Task<Embed> ResumeAsync(IGuild guild)
        {
            try
            {
                var player = _lavaNode.GetPlayer(guild);

                if (player.PlayerState is PlayerState.Paused)
                {
                    await player.ResumeAsync();
                }

                return await Helper.CreateBasicEmbed("", $"**{player.Track.Title}** has been resumed.", Color.Blue);
            }
            catch (InvalidOperationException ex)
            {
                return await Helper.CreateErrorEmbed(ex.Message);
            }
        }

        /// <summary>Fetches lyrics from OVH API and returns an embed containing the lyrics.
        /// </summary>
        public async Task<Embed> FetchLyricsAsync(IGuild guild)
        {
            var player = _lavaNode.GetPlayer(guild);

            if (player == null || player.Track == null)
                return await Helper.CreateErrorEmbed("No music playing.");

            string lyrics = await player.Track.FetchLyricsFromOVHAsync();

            //Create request to specified url
            //HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://api.ksoft.si/lyrics/search?q=" + player.Track.Title);
            //request.Headers["Authorization"] = "";
            //request.Method = "GET";

            //string httpResponse = await Helper.HttpRequestAndReturnJson(request);
            
            //JObject jsonParsed = JObject.Parse(httpResponse);

            //string songName = (string)jsonParsed["data"][0]["name"];
            //string artist = (string)jsonParsed["data"][0]["artist"];
            //string lyrics = (string)jsonParsed["data"][0]["lyrics"];

            if (lyrics == "")
                return await Helper.CreateErrorEmbed("Couldn't fetch lyrics.");

            //return await Helper.CreateBasicEmbed($"{artist} - {songName} lyrics", lyrics, Color.Blue);
            return await Helper.CreateBasicEmbed($"{player.Track.Title} lyrics", lyrics, Color.Blue);
        }

        /// <summary>Method called when OnTrackEnded event is fired.
        /// </summary>
        private async Task OnTrackEnded(TrackEndedEventArgs args)
        {
            var player = args.Player;

            if (!args.Reason.ShouldPlayNext())
                return;

            if (!player.Queue.TryDequeue(out var queueable))
            {
                await player.StopAsync();
                player.Queue.Clear();
                await _lavaNode.LeaveAsync(args.Player.VoiceChannel);
            }

            if (!(queueable is LavaTrack track))
                return;          

            await args.Player.PlayAsync(track);
            await args.Player.TextChannel.SendMessageAsync(
                embed: await Helper.CreateBasicEmbed("Now Playing", $"[{track.Title}]({track.Url})", Color.Blue));
        }
    }
}