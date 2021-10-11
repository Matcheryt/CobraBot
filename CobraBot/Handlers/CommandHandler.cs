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
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CobraBot.Common.EmbedFormats;
using CobraBot.Database;
using CobraBot.TypeReaders;
using Discord;
using Discord.Addons.Hosting;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Serilog;

namespace CobraBot.Handlers
{
    public class CommandHandler : DiscordClientService
    {
        private readonly BotContext _botContext;
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commandService;
        private readonly IServiceProvider _services;

        //Constructor
        public CommandHandler(DiscordSocketClient client, ILogger<CommandHandler> logger, IServiceProvider services,
            CommandService commandService, BotContext botContext) : base(client, logger)
        {
            _commandService = commandService;
            _client = client;
            _services = services;
            _botContext = botContext;

            //Handle events
            _client.MessageReceived += HandleCommandAsync;
            _commandService.CommandExecuted += OnCommandExecuted;
        }


        //Adds modules and services
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //Adds custom type readers
            _commandService.AddTypeReader(typeof(IUser), new ExtendedUserTypeReader());
            _commandService.AddTypeReader(typeof(IRole), new ExtendedRoleTypeReader());

            await _commandService.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }

        //Called whenever a user sends a message
        private async Task HandleCommandAsync(SocketMessage rawMessage)
        {
            //If msg == null or if the message was sent by another bot, then return
            if (rawMessage is not SocketUserMessage msg || msg.Author.IsBot)
                return;

            var argPos = 0;


            //If the message is received on the bot's DM channel, then we ignore it
            //as we only want to process commands used on servers
            if (msg.Channel is not ITextChannel guildTextChannel)
                return;

            //Tries to get guild custom prefix, if guild doesn't have one, then prefix == '-' (default bot prefix)
            var prefix = _botContext.GetGuildPrefix(guildTextChannel.Guild.Id);

            //Check if the message sent has the specified prefix
            if (!msg.HasStringPrefix(prefix, ref argPos) &&
                !msg.HasMentionPrefix(_client.CurrentUser, ref argPos)) return;

            //Check if the message contains only the prefix, if it does we return as it isn't a command
            if (msg.Content.Length == prefix.Length)
                return;

            //Create context
            var context = new SocketCommandContext(_client, msg);

            //If the message received has the command prefix, then we execute the command
            await _commandService.ExecuteAsync(context, argPos, _services);
        }


        //Handle command post execution
        private async Task OnCommandExecuted(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            //If command was executed successfully, then log it to the console
            if (result.IsSuccess)
            {
                Log.Information($"{context.User} has used '{context.Message}' on {context.Guild}.");
            }
            //Else, if command execution failed, handle the error
            else
            {
                if (string.IsNullOrEmpty(result.ErrorReason))
                    return;

                try
                {
                    switch (result.Error)
                    {
                        case CommandError.ObjectNotFound:
                            await SendErrorMessage(context, $"**{result.ErrorReason}**");
                            break;

                        case CommandError.UnmetPrecondition:
                            await SendErrorMessage(context, "**No permission!**\n" + result.ErrorReason);
                            break;

                        case CommandError.BadArgCount:
                            var parametersList = command.Value.Parameters.Select(x => x.Name).ToList();
                            await SendErrorMessage(context, parametersList.Any()
                                ? $"**Missing Parameters!** Command usage: `{_botContext.GetGuildPrefix(context.Guild.Id)}{command.Value.Aliases[0]} [{string.Join(", ", parametersList)}]`"
                                : $"**Missing Parameters!** Command usage: `{_botContext.GetGuildPrefix(context.Guild.Id)}{command.Value.Aliases[0]}`");
                            break;

                        case CommandError.Exception:
                            await SendErrorMessage(context,
                                "An error occurred, please report it to Matcher#0183\n" + result.ErrorReason);
                            break;

                        case CommandError.ParseFailed:
                            await SendErrorMessage(context, "**Parse Failed!** Please check command syntax");
                            break;

                        case CommandError.MultipleMatches:
                            await SendErrorMessage(context, "**Multiple Matches!**");
                            break;

                        case CommandError.Unsuccessful:
                            await SendErrorMessage(context,
                                "**Command execution unsuccessful!** Please report this to Matcher#0183");
                            break;
                    }
                }
                catch (Exception)
                {
                    //If the bot doesn't have permission to send any of the above messages to the channel, then just suppress the error
                    //as it isn't our problem if the bot can't send those messages to the channel
                }
            }
        }

        /// <summary> Sends an error message to the channel where the command was issued. </summary>
        /// <param name="context"> The command context. </param>
        /// <param name="errorMessage"> The error message to show. </param>
        private static async Task SendErrorMessage(ICommandContext context, string errorMessage)
        {
            var errorEmbed = CustomFormats.CreateErrorEmbed(errorMessage);
            await context.Channel.SendMessageAsync(embed: errorEmbed);
        }
    }
}