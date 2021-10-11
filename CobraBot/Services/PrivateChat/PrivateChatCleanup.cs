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

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CobraBot.Database;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace CobraBot.Services.PrivateChat
{
    public class PrivateChatCleanup : IHostedService
    {
        private readonly BotContext _botContext;
        private readonly DiscordSocketClient _client;
        private Timer _timer;

        public PrivateChatCleanup(BotContext botContext, DiscordSocketClient client)
        {
            _botContext = botContext;
            _client = client;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(async _ => await CleanupPrivateChannels(), null, TimeSpan.FromSeconds(10),
                TimeSpan.FromMinutes(30));
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer.Change(Timeout.InfiniteTimeSpan, TimeSpan.Zero);
            return Task.CompletedTask;
        }

        /// <summary> Method called every 30 minutes to delete unused private channels. </summary>
        private async Task CleanupPrivateChannels()
        {
            Log.Logger.Information("Started private channel cleanup.");

            var deletedChannels = 0;

            var privateChats = await _botContext.PrivateChats.AsAsyncEnumerable().ToListAsync();

            if (!privateChats.Any())
            {
                Log.Logger.Information("Private channel cleaning aborted - No private chats found to clean.");
                return;
            }

            //Loop through every private chat
            foreach (var privateChat in privateChats)
            {
                //Get the guild for that private chat
                var guild = _client.GetGuild(privateChat.GuildId);

                //Assert that guild isnt null
                if (guild == null)
                    continue;

                //Get the voice channel associated with private chat entry in database
                var channel = guild.GetVoiceChannel(privateChat.ChannelId);

                //If the channel isn't on the guild anymore, then delete the database entry
                if (channel == null)
                {
                    _botContext.Remove(privateChat);
                    continue;
                }

                //Count every user in the channel (except bots)
                var channelUserCount = channel.Users.Count(u => !u.IsBot);

                //If there is one or more users in the channel, continue
                if (channelUserCount >= 1) continue;

                //If there is less than 1 user in the channel, then delete the channel and delete the entry from the db
                await channel.DeleteAsync(new RequestOptions
                    { AuditLogReason = "Delete private channel as it is empty" });

                deletedChannels++;

                _botContext.Remove(privateChat);
            }

            //Save changes
            await _botContext.SaveChangesAsync();

            Log.Logger.Information($"Private channels cleanup deleted {deletedChannels} channels.");
        }
    }
}