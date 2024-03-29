﻿/*
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
using System.Reflection;
using System.Threading.Tasks;
using CobraBot.Database;
using CobraBot.Handlers;
using CobraBot.Services;
using CobraBot.Services.Moderation;
using CobraBot.Services.PrivateChat;
using Discord;
using Discord.Addons.Hosting;
using Discord.Commands;
using Discord.WebSocket;
using EFCoreSecondLevelCacheInterceptor;
using Interactivity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace CobraBot
{
    public class Program
    {
        private readonly DiscordSocketClient _client;
        private readonly IHost _host;

        public Program()
        {
            //Build the host
            _host = CreateHostBuilder().Build();

            //Get the discord client from the host
            _client = _host.Services.GetRequiredService<DiscordSocketClient>();

            //Handle events
            _client.Ready += Client_Ready;
        }

        private static void Main()
        {
            new Program().StartAsync().GetAwaiter().GetResult();
        }

        //Run the host
        public async Task StartAsync()
        {
            await _host.RunAsync();
        }


        //Configure the host builder with services and logging
        public static IHostBuilder CreateHostBuilder()
        {
            var hostBuilder = Host.CreateDefaultBuilder()
                .UseSerilog((_, config) =>
                {
                    config.WriteTo.Console(
                        outputTemplate: "{Timestamp:dd-MM-yyyy HH:mm:ss} [{Level}] {Message}{NewLine}{Exception}");

                    if (!string.IsNullOrEmpty(Configuration.SentryApiKey))
                        config.WriteTo.Sentry(x =>
                        {
                            x.Dsn = Configuration.SentryApiKey;
                            x.MinimumBreadcrumbLevel = LogEventLevel.Debug;
                            x.MinimumEventLevel = LogEventLevel.Warning;
                        });

                    config.MinimumLevel.Information();
                    config.MinimumLevel.Override("Microsoft", LogEventLevel.Error);
                })
                .ConfigureDiscordHost((_, config) =>
                {
                    config.SocketConfig = new DiscordSocketConfig
                    {
                        MessageCacheSize = 100,
                        AlwaysDownloadUsers = true,
                        LogLevel = LogSeverity.Info,
                        GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.GuildMembers
                    };

                    config.Token = Configuration.DevelopToken;
                })
                .UseCommandService((_, config) =>
                {
                    config.DefaultRunMode = RunMode.Async;
                    config.CaseSensitiveCommands = false;
                    config.IgnoreExtraArgs = false;
                })
                .ConfigureServices((_, services) =>
                {
                    services
                        .AddHostedService<CommandHandler>()
                        .AddMemoryCache()
                        .AddEFSecondLevelCache(options =>
                        {
                            options
                                .DisableLogging(true)
                                .UseMemoryCacheProvider()
                                .CacheAllQueries(CacheExpirationMode.Sliding, TimeSpan.FromHours(12));
                        })
                        .AddDbContext<BotContext>((serviceProvider, options) =>
                        {
                            options.UseSqlite("Data Source=CobraDB.db");
                            options.AddInterceptors(serviceProvider.GetRequiredService<SecondLevelCacheInterceptor>());
                        }, ServiceLifetime.Transient)
                        .AddSingleton<InteractivityService>()
                        .AddSingleton(new InteractivityConfig { RunOnGateway = false })
                        .AddSingleton<PrivateChatService>()
                        .AddHostedService<PrivateChatCleanup>()
                        .AddSingleton<ModerationService>()
                        .AddSingleton<LookupService>()
                        .AddSingleton<ApiService>()
                        .AddSingleton<FunService>()
                        .AddSingleton<NsfwService>()
                        .AddSingleton<InfoService>()
                        .AddSingleton<UtilitiesService>()
                        .AddSingleton<SetupService>();
                });

            return hostBuilder;
        }


        //When bot is ready
        private async Task Client_Ready()
        {
            //Following instruction sets bot "Playing" status
            const string game = "-help";
            await _client.SetGameAsync($"{game}", null, ActivityType.Listening);

            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine($@"
   ____      _                 ____        _   
  / ___|___ | |__  _ __ __ _  | __ )  ___ | |_ 
 | |   / _ \| '_ \| '__/ _` | |  _ \ / _ \| __|
 | |__| (_) | |_) | | | (_| | | |_) | (_) | |_ 
  \____\___/|_.__/|_|  \__,_| |____/ \___/ \__|
                      
                         Version {Assembly.GetEntryAssembly()?.GetName().Version?.ToString(2)}
");
            Console.ResetColor();
            Console.WriteLine("'" + game + "'" + " has been defined as Cobra's currently playing 'game'");
            Console.WriteLine($"I'm now online on {_client.Guilds.Count} guilds\n");
        }
    }
}