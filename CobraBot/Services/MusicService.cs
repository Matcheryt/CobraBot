using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Victoria;
using Victoria.EventArgs;
using Victoria.Enums;
using Discord.Commands;
using CobraBot.Helpers;
using System.Net;
using CobraBot.Handlers;
using Interactivity;
using Interactivity.Pagination;
using Newtonsoft.Json.Linq;

namespace CobraBot.Services
{
    public sealed class MusicService
    {
        private readonly LavaNode _lavaNode;
        private readonly InteractivityService _interactivityService;

        public MusicService(LavaNode lavaNode, InteractivityService interactivityService)
        {
            _lavaNode = lavaNode;
            _interactivityService = interactivityService;

            //Events
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
            if (oldState.VoiceChannel == (newState.VoiceChannel))
                return;

            //We count every user in the channel that isn't a bot, and put that result in 'users' variable
            int users = oldState.VoiceChannel.Users.Count(u => !u.IsBot);

            //If there are no users left in the voice channel, we make the bot leave
            if (users < 1)
            {
                var player = _lavaNode.GetPlayer(((SocketGuildUser)user).Guild);
                await player.StopAsync();                
                await _lavaNode.LeaveAsync(player.VoiceChannel);
            }
        }

        /// <summary>Joins the voice channel the user is in and reacts to user message with 'okEmoji'.
        /// </summary>
        public async Task JoinAsync(SocketCommandContext context)
        {
            var guild = context.Guild;
            var voiceState = (IVoiceState)context.User;
            var textChannel = (ITextChannel) context.Channel;

            var okEmoji = new Emoji("👍");

            //If the user isn't connected to a voice channel
            if (voiceState == null)
            {
                await context.Channel.SendMessageAsync(
                    embed: await Helper.CreateErrorEmbed("You must be connected to a voice channel!"));
                return;
            }

            //If bot is already connected to a voice channel, do nothing and return
            if (_lavaNode.HasPlayer(guild))
                return;

            //If user is connected to a voice channel, and the bot isn't connected to it
            //Then try to join
            try
            {
                await _lavaNode.JoinAsync(voiceState.VoiceChannel, textChannel);
                await context.Message.AddReactionAsync(okEmoji);
            }
            catch (Exception ex)
            {
                await context.Channel.SendMessageAsync(embed: await Helper.CreateErrorEmbed(ex.Message));
            }
        }

        /// <summary>Makes bot leave voice channel and reacts to user message with 'byeEmoji'.
        /// </summary>
        public async Task LeaveAsync(SocketCommandContext context)
        {
            var guild = context.Guild;

            var byeEmoji = new Emoji("👋");

            if (!_lavaNode.HasPlayer(guild))
            {
                await context.Message.AddReactionAsync(byeEmoji);
                return;
            }

            try
            {
                //Get The Player Via GuildID.
                var player = _lavaNode.GetPlayer(guild);

                //if The Player is playing, Stop it.
                if (player.PlayerState is PlayerState.Playing)
                    await player.StopAsync();

                //Leave the voice channel.
                await _lavaNode.LeaveAsync(player.VoiceChannel);

                await context.Message.AddReactionAsync(byeEmoji);
            }
            //Tell the user about the error so they can report it back to us.
            catch (Exception ex)
            {
                await context.Channel.SendMessageAsync(embed: await Helper.CreateErrorEmbed(ex.Message));
            }
        }

        /// <summary>Plays the requested song or adds it to the queue.
        /// <para>It also joins the voice channel if the bot isn't already joined.</para>
        /// </summary>
        public async Task PlayAsync(SocketCommandContext context, string query)
        {
            var user = (SocketGuildUser)context.User;
            var guild = context.Guild;

            //Check If User Is Connected To Voice channel.
            if (user.VoiceChannel == null)
            {
                await context.Channel.SendMessageAsync(
                    embed: await Helper.CreateErrorEmbed("You must be connected to a voice channel!"));
                return;
            }
              
            //Check the guild has a player available.
            if (!_lavaNode.HasPlayer(guild))
            {
                //If it doesn't, then it means the bot isn't connected to a voice channel,
                //so we make the bot join a voice channel in order for play command to work
                var voiceState = (IVoiceState) context.User;
                var textChannel = (ITextChannel) context.Channel;
                await _lavaNode.JoinAsync(voiceState.VoiceChannel, textChannel);
            }

            try
            {
                //Get the player for that guild.
                var player = _lavaNode.GetPlayer(guild);
                
                LavaTrack track;

                //Find The Youtube Track the User requested.
                var search = Uri.IsWellFormedUriString(query, UriKind.Absolute) ?
                    await _lavaNode.SearchAsync(query)
                    : await _lavaNode.SearchYouTubeAsync(query);               

                //If we couldn't find anything, tell the user.
                if (search.LoadStatus == LoadStatus.NoMatches)
                {
                    await context.Channel.SendMessageAsync(
                        embed: await Helper.CreateErrorEmbed($"No results found for {query}."));
                    return;
                }

                //If results derive from search results (ex: ytsearch: some song)
                if (search.LoadStatus == LoadStatus.SearchResult)
                {
                    //Then load the first track of the search results
                    track = search.Tracks.FirstOrDefault();

                    //If the Bot is already playing music, or if it is paused but still has music in the playlist, Add the requested track to the queue.
                    if (player.Track != null && (player.PlayerState is PlayerState.Playing || player.PlayerState is PlayerState.Paused))
                    {
                        player.Queue.Enqueue(track);
                        await context.Channel.SendMessageAsync(embed: await Helper.CreateBasicEmbed("",
                            $"**{track.Title}** has been added to queue. [{context.User.Mention}]", Color.Blue));
                        return;
                    }

                    //Player was not playing anything, so lets play the requested track.
                    await player.PlayAsync(track);

                    await context.Channel.SendMessageAsync(embed: await Helper.CreateBasicEmbed("Now playing",
                        $"[{track.Title}]({track.Url})", Color.Blue));
                    
                    return;
                }
                
                //If results derive from a playlist,
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
                    await context.Channel.SendMessageAsync(embed: await Helper.CreateBasicEmbed("",
                        $"**{search.Tracks.Count} tracks** have been added to queue. [{context.User.Mention}]",
                        Color.Blue));
                    
                    return;
                }
                
                //If the player isn't playing anything
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
                await context.Channel.SendMessageAsync(embed: await Helper.CreateBasicEmbed("Now playing",
                    $"[{track.Title}]({track.Url})\n{search.Tracks.Count - 1} other tracks have been added to queue.",
                    Color.Blue));
            }
            //If after all the checks we did, something still goes wrong. Tell the user about it so they can report it back to us.
            catch (Exception ex)
            {
                await context.Channel.SendMessageAsync(embed: await Helper.CreateErrorEmbed(ex.Message));
            }
        }

        /// <summary>Shuffles queue and returns an embed.
        /// </summary>
        public async Task ShuffleAsync(SocketCommandContext context)
        {
            var guild = context.Guild;

            var shuffleEmoji = new Emoji("🔀");

            //Checks if bot is connected to a voice channel
            if (!_lavaNode.HasPlayer(guild))
            {
                await context.Channel.SendMessageAsync(embed: await Helper.CreateErrorEmbed("Player doesn't seem to be playing anything right now."));
                return;
            }

            //Bot is connected to voice channel, so we get the player associated with the guild
            var player = _lavaNode.GetPlayer(guild);

            //Check if the queue is > 1
            if (player.Queue.Count <= 1)
            {
                await context.Channel.SendMessageAsync(embed: await Helper.CreateErrorEmbed("No songs in queue to shuffle!"));
                return;
            }

            //If all conditions check, then shuffle the queue and react to sent message with shuffleEmoji
            player.Queue.Shuffle();
            await context.Message.AddReactionAsync(shuffleEmoji);
        }

        /// <summary>Returns an embed containing the player queue.
        /// </summary>
        public async Task QueueAsync(SocketCommandContext context)
        {
            var guild = context.Guild;

            //Checks if bot is connected to a voice channel
            if (!_lavaNode.HasPlayer(guild))
            {
                await context.Channel.SendMessageAsync(embed: await Helper.CreateErrorEmbed("Could not acquire player."));
                return;
            }

            //Bot is connected to voice channel, so we get the player associated with the guild
            var player = _lavaNode.GetPlayer(guild);

            //If player isn't playing, then we return
            if (!(player.PlayerState is PlayerState.Playing))
            {
                await context.Channel.SendMessageAsync(embed: await Helper.CreateErrorEmbed("I'm not playing anything right now."));
                return;
            }

            //If there are no more songs in queue except for the current playing song, we return with a reply
            //saying the currently playing song and that no more songs are queued
            if (player.Queue.Count < 1 && player.Track != null)
            {
                await context.Channel.SendMessageAsync(embed: await Helper.CreateBasicEmbed("", $"**Now playing: {player.Track.Title}**\nNo more songs queued.", Color.Blue));
                return;
            }

            try
            {
                //After checking if we have tracks in the queue

                //We save the count of tracks in queue to tracksCount variable
                var tracksCount = player.Queue.Count;

                /* We calculate the maximum items per page we want
                   this will return the minimum number, either 10 or tracksCount*/
                var maxItemsPerPage = Math.Min(10, tracksCount);

                //We calculate how many pages we'll have (used to initialize array)
                var maxPages = tracksCount / maxItemsPerPage;

                //We initialize an array with size of maxPages
                var pages = new PageBuilder[maxPages];

                var trackNum = 2; //trackNum == 2 because we're not including the first track

                //We itterate through all the pages we need
                for (int i = 0; i < maxPages; i++)
                {
                    var descriptionBuilder = new StringBuilder();

                    //We take X items, equal to the number of maxItemsPerPage, so we don't overflow the embed max description length
                    var tracks = player.Queue.Skip(i).Take(maxItemsPerPage);

                    //We itterate through the tracks taken on the previous instruction
                    foreach (var track in tracks)
                    {
                        //We create the description for each page
                        descriptionBuilder.Append($"{trackNum}: [{track.Title}]({track.Url})\n");
                        trackNum++;
                    }

                    //We create the page, with the description created on the previous loop
                    pages[i] = new PageBuilder().WithTitle($"Now playing: {player.Track.Title}").WithDescription($"{descriptionBuilder}").WithColor(Color.Blue);
                }

                //We create the paginator to send
                var paginator = new StaticPaginatorBuilder()
                    .WithUsers(context.User)
                    .WithFooter(PaginatorFooter.PageNumber)
                    .WithEmotes(new Dictionary<IEmote, PaginatorAction>()
                    {
                        { new Emoji("⏮️"), PaginatorAction.SkipToStart },
                        { new Emoji("⬅️"), PaginatorAction.Backward },
                        { new Emoji("➡️"), PaginatorAction.Forward },
                        { new Emoji("⏭️"), PaginatorAction.SkipToEnd }
                    })
                    .WithPages(pages)
                    .WithTimoutedEmbed(pages[0].Build().Embed.ToEmbedBuilder())
                    .Build();

                //Send the paginator to the text channel
                await _interactivityService.SendPaginatorAsync(paginator, context.Channel, TimeSpan.FromSeconds(150));
            }
            catch (Exception ex)
            {
                await context.Channel.SendMessageAsync(embed: await Helper.CreateErrorEmbed(ex.Message));
            }

        }

        /// <summary>Removes specified track from queue and returns an embed.
        /// </summary>
        public async Task<Embed> RemoveFromQueueAsync(IGuild guild, int index, int indexMax)
        {           
            if (!_lavaNode.HasPlayer(guild))
                return await Helper.CreateErrorEmbed("Could not acquire player.");

            var player = _lavaNode.GetPlayer(guild);

            /* We decrement 2 to the index, as queue command shows first song in queue with number 2
               and first item in queue has an index of 0 */
            index -= 2;

            if (player.Queue.ElementAt(index) == null)
                return await Helper.CreateErrorEmbed("There is no song in queue with specified index!");

            try 
            {
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
                if (!_lavaNode.HasPlayer(guild))
                    return await Helper.CreateErrorEmbed("Could not acquire player.");

                var player = _lavaNode.GetPlayer(guild);

                if (player.Queue.Count < 1)
                    return await Helper.CreateErrorEmbed("Unable to skip a track as there is only one or no songs currently playing.");

                try
                {
                    //Save the current song for use after we skip it.
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
            catch (Exception ex)
            {
                return await Helper.CreateErrorEmbed(ex.Message);
            }
        }

        /// <summary>Stops playback, clears queue, makes bot leave channel, and reacts to user message with 'stopEmoji'.
        /// </summary>
        public async Task StopAsync(SocketCommandContext context)
        {
            var guild = context.Guild;

            var stopEmoji = new Emoji("🛑");

            try
            {
                if (!_lavaNode.HasPlayer(guild))
                {
                    await context.Channel.SendMessageAsync(embed: await Helper.CreateErrorEmbed("Could not acquire player."));
                    return;
                }

                var player = _lavaNode.GetPlayer(guild);

                //If the player is playing, stop its playback
                if (player.PlayerState is PlayerState.Playing)
                    await player.StopAsync();

                //Clears the queue and makes bot leave channel
                player.Queue.Clear();
                await _lavaNode.LeaveAsync(player.VoiceChannel);

                await context.Message.AddReactionAsync(stopEmoji);
            }
            catch (Exception ex)
            {
                await context.Channel.SendMessageAsync(embed: await Helper.CreateErrorEmbed(ex.Message));
            }
        }


        /// <summary>Pauses current track and returns an embed.
        /// </summary>
        public async Task<Embed> PauseAsync(IGuild guild)
        {
            try
            {
                if (!_lavaNode.HasPlayer(guild))
                    return await Helper.CreateErrorEmbed("Could not acquire player.");

                var player = _lavaNode.GetPlayer(guild);
                if (!(player.PlayerState is PlayerState.Playing))
                    return await Helper.CreateErrorEmbed("There is nothing to pause.");

                await player.PauseAsync();
                return await Helper.CreateBasicEmbed("", $"**{player.Track.Title}** has been paused.", Color.Blue);
            }
            catch (Exception ex)
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
                if (!_lavaNode.HasPlayer(guild))
                    return await Helper.CreateErrorEmbed("Could not acquire player.");

                var player = _lavaNode.GetPlayer(guild);

                if (player.PlayerState is PlayerState.Paused)
                    await player.ResumeAsync();

                return await Helper.CreateBasicEmbed("", $"**{player.Track.Title}** has been resumed.", Color.Blue);
            }
            catch (Exception ex)
            {
                return await Helper.CreateErrorEmbed(ex.Message);
            }
        }

        public async Task SearchAsync(string searchString, SocketCommandContext context)
        {
            var searchResults = await _lavaNode.SearchYouTubeAsync(searchString);

            if (searchResults.Tracks.Count == 0)
            {
                await context.Channel.SendMessageAsync(
                    embed: await Helper.CreateErrorEmbed($"No results found for {searchString}."));
                return;
            }

            var resultsCount = searchResults.Tracks.Count;

            /* We calculate the maximum items per page we want
                   this will return the minimum number, either 10 or resultsCount*/
            var maxItemsPerPage = Math.Min(10, resultsCount);

            var maxPages = resultsCount / maxItemsPerPage;

            var pages = new PageBuilder[maxPages];

            var trackNum = 0;

            //We itterate through all the pages we need
            for (int i = 0; i < maxPages; i++)
            {
                var descriptionBuilder = new StringBuilder();

                //We take X items, equal to the number of maxItemsPerPage, so we don't overflow the embed max description length
                var results = searchResults.Tracks.Skip(i).Take(maxItemsPerPage);

                //We itterate through the results taken on the previous instruction
                foreach (var result in results)
                {
                    //We create the description for each page
                    descriptionBuilder.Append($"**{trackNum+1}) {result.Title}** - {result.Duration}\n");
                    trackNum++;
                }

                //We create the page, with the description created on the previous loop
                pages[i] = new PageBuilder().WithDescription($"{descriptionBuilder}").WithColor(Color.Purple);
            }

            //We create the paginator to send
            var paginator = new StaticPaginatorBuilder()
                .WithUsers(context.User)
                .WithFooter(PaginatorFooter.PageNumber)
                .WithEmotes(new Dictionary<IEmote, PaginatorAction>()
                {
                    { new Emoji("⏮️"), PaginatorAction.SkipToStart },
                    { new Emoji("⬅️"), PaginatorAction.Backward },
                    { new Emoji("➡️"), PaginatorAction.Forward },
                    { new Emoji("⏭️"), PaginatorAction.SkipToEnd }
                })
                .WithPages(pages)
                .WithTimoutedEmbed(pages[0].Build().Embed.ToEmbedBuilder())
                .Build();

            //Send paginator to text channel
            await _interactivityService.SendPaginatorAsync(paginator, context.Channel, TimeSpan.FromSeconds(90));
        }

        /// <summary>Fetches lyrics from OVH API and returns an embed containing the lyrics.
        /// </summary>
        public async Task<Embed> FetchLyricsAsync(IGuild guild)
        {
            var player = _lavaNode.GetPlayer(guild);

            if (player?.Track == null)
                return await Helper.CreateErrorEmbed("No music playing.");

            try
            {
                //Create request to specified url
                var request = (HttpWebRequest)WebRequest.Create("https://api.ksoft.si/lyrics/search?q=" + player.Track.Title);
                request.Headers["Authorization"] = $"Bearer {Configuration.KSoftApiKey}";
                request.Method = "GET";

                string httpResponse = await Helper.HttpRequestAndReturnJson(request);

                var jsonParsed = JObject.Parse(httpResponse);

                string songName = (string)jsonParsed["data"][0]["name"];
                string artist = (string)jsonParsed["data"][0]["artist"];
                string lyrics = (string)jsonParsed["data"][0]["lyrics"];

                var embed = new EmbedBuilder()
                    .WithTitle($"{artist} - {songName} lyrics")
                    .WithDescription(lyrics)
                    .WithColor(Color.Blue)
                    .WithFooter("Powered by KSoft.Si").Build();

                return embed;
            }
            catch (Exception)
            {
                return await Helper.CreateErrorEmbed("Couldn't fetch lyrics.");
            }

            //Uncomment the following lines if you don't have a KSoft account and wish to use Victoria's lyrics method
            /*string lyrics = await player.Track.FetchLyricsFromOVHAsync();

            if (lyrics == "")
                return await Helper.CreateErrorEmbed("Couldn't fetch lyrics.");

            return await Helper.CreateBasicEmbed($"{player.Track.Title} lyrics", lyrics, Color.Blue);*/
        }

        /// <summary>Method called when OnTrackEnded event is fired.
        /// </summary>
        private async Task OnTrackEnded(TrackEndedEventArgs args)
        {
            //Get player and text channel from the player that triggered the event
            var player = args.Player;
            var textChannel = args.Player.TextChannel;

            //If we shouldn't play next track
            if (!args.Reason.ShouldPlayNext())
                return; //Then return

            //If we haven't something to play (queue is empty)
            if (!player.Queue.TryDequeue(out var queueable))
            {
                //Stop player, clear the queue and make the bot leave the voice channel
                await player.StopAsync();
                player.Queue.Clear();
                await _lavaNode.LeaveAsync(args.Player.VoiceChannel);
            }

            var track = queueable;

            //If after all the checks, we have something to play
            await args.Player.PlayAsync(track); //Play next track

            /* Send "Now Playing" message to text channel, and delete it after the music ends 
               (this prevents bot spamming "Now playing" messages when queue is long) */
            _interactivityService.DelayedSendMessageAndDeleteAsync(textChannel, null, queueable.Duration, null, false, await Helper.CreateBasicEmbed("Now Playing", $"[{track.Title}]({track.Url})", Color.Blue));
        }
    }
}