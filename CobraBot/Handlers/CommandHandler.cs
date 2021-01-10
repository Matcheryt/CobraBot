using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CobraBot.Common.EmbedFormats;
using CobraBot.Database;
using CobraBot.Services;
using CobraBot.TypeReaders;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace CobraBot.Handlers
{
    public class CommandHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly IServiceProvider _services;
        private readonly CommandService _commands;
        private readonly BotContext _botContext;

        //Constructor
        public CommandHandler(IServiceProvider services, BotContext botContext)
        {
            _commands = services.GetRequiredService<CommandService>();
            _client = services.GetRequiredService<DiscordSocketClient>();
            _services = services;
            _botContext = botContext;
            
            //Handle events
            _client.MessageReceived += HandleCommandAsync;
            _commands.CommandExecuted += OnCommandExecuted;
        }


        //Adds modules and services
        public async Task InitializeAsync()
        {
            _commands.AddTypeReader(typeof(IUser), new ExtendedUserTypeReader<IUser>());
            _commands.AddTypeReader(typeof(IRole), new ExtendedRoleTypeReader<IRole>());

            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }


        //Called whenever a user sends a message
        private async Task HandleCommandAsync(SocketMessage rawMessage)
        {
            //If msg == null or if the message was sent by another bot, then return
            if ((rawMessage is not SocketUserMessage msg) || msg.Author.IsBot)
                return;

            int argPos = 0;

            var context = new SocketCommandContext(_client, msg);

            //If the message is received on the bot's DM channel, then we ignore it
            //as we only want to process commands used on servers
            if (msg.Channel is IPrivateChannel)
                return;
            
            //Tries to get guild custom prefix, if guild doesn't have one, then prefix == '-' (default bot prefix)
            var prefix = _botContext.GetGuildPrefix(context.Guild.Id);

            //Check if the message sent has the specified prefix
            if (!msg.HasStringPrefix(prefix, ref argPos)) return;

            //If the message received has the command prefix, then we execute the command
            await _commands.ExecuteAsync(context, argPos, _services);
        }


        //Handle command post execution
        private async Task OnCommandExecuted(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            //If command was executed successfully, then log it to the console
            if (result.IsSuccess)
            {
                await LoggingService.LogAsync(new LogMessage(LogSeverity.Info, "Command",
                    $"{context.User} has used {context.Message} on {context.Guild}."));
            }
            //Else, if command execution failed, handle the error
            else
            {
                try
                {
                    switch (result.Error)
                    {
                        case CommandError.ObjectNotFound:
                            if (command.Value.Name == "User info")
                            {
                                await context.Channel.SendMessageAsync(embed: CustomFormats.CreateErrorEmbed("**User not found!**"));
                                break;
                            }
                            await context.Channel.SendMessageAsync(embed: CustomFormats.CreateErrorEmbed("**Object not found!**"));
                            break;

                        case CommandError.UnmetPrecondition:
                            await context.Channel.SendMessageAsync(embed: CustomFormats.CreateErrorEmbed("**No permission!**\n" + result.ErrorReason));
                            break;

                        case CommandError.BadArgCount:
                            var param = command.Value.Parameters.Select(x => x.Name);
                            await context.Channel.SendMessageAsync(embed: CustomFormats.CreateErrorEmbed(
                                $"**Missing Parameters!** Command usage: `{_botContext.GetGuildPrefix(context.Guild.Id)}{command.Value.Aliases[0]} [{string.Join(", ", param)}]`"));
                            break;

                        case CommandError.Exception:
                            await context.Channel.SendMessageAsync(embed: CustomFormats.CreateErrorEmbed("An error occurred, please report it to Matcher#0183\n" + result.ErrorReason));
                            break;

                        case CommandError.ParseFailed:
                            await context.Channel.SendMessageAsync(embed: CustomFormats.CreateErrorEmbed("**Parse Failed!** Please check command syntax"));
                            break;

                        case CommandError.MultipleMatches:
                            await context.Channel.SendMessageAsync(embed: CustomFormats.CreateErrorEmbed("**Multiple Matches!**"));
                            break;

                        case CommandError.Unsuccessful:
                            await context.Channel.SendMessageAsync(embed: CustomFormats.CreateErrorEmbed("**Command execution unsuccessful!** Please report this to Matcher#0183"));
                            break;

                        case CommandError.UnknownCommand:
                            await context.Channel.SendMessageAsync(embed: CustomFormats.CreateErrorEmbed($"**Unknown Command:** Type `{_botContext.GetGuildPrefix(context.Guild.Id)}help` to see available commands."));
                            break;
                    }
                }
                catch(Exception)
                {
                    //If the bot doesn't have permission to send any of the above messages to the channel, then just suppress the error
                    //as it isn't our problem if the bot can't send those messages to the channel
                }
            }

        }
    }
}