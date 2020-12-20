using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CobraBot.Common;
using CobraBot.Database;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Z.EntityFramework.Plus;

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
        }

        public async Task InitializeAsync()
        {
            await _commands.AddModulesAsync(
                assembly: Assembly.GetEntryAssembly(),
                services: _services);
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
            if (context.IsPrivate)
                return;

            var guildSettings = _botContext.Guilds.AsNoTracking().Where(x => x.GuildId == context.Guild.Id).FromCache(context.Guild.Id.ToString()).FirstOrDefault();

            var savedPrefix = guildSettings?.CustomPrefix;
            
            //CustomPrefix to be used
            string prefix;

            //If there isn't a saved prefix for specified guild, then use default prefix
            if (savedPrefix == null)
            {
                prefix = "-";
                if (!msg.HasStringPrefix(prefix, ref argPos)) return;
            }
            //If there is a saved prefix, use it as the prefix
            else
            {
                prefix = savedPrefix;
                if (!msg.HasStringPrefix(prefix, ref argPos)) return;
            }

            //If the message received, has the command prefix, then we execute the command
            var result = await _commands.ExecuteAsync(context, argPos, _services);

            //If any errors happen while executing the command, they are handled here
            #region ErrorHandling
            if (!result.IsSuccess && result.Error != CommandError.UnknownCommand)
            {
                switch (result.Error)
                {
                    case CommandError.ObjectNotFound:
                        if (msg.Content.Contains("usinfo"))
                        {
                            await context.Channel.SendMessageAsync(embed: EmbedFormats.CreateErrorEmbed("**User not found!**"));
                            break;
                        }
                        await context.Channel.SendMessageAsync(embed: EmbedFormats.CreateErrorEmbed("**Object not found!**"));
                        break;

                    case CommandError.UnmetPrecondition:
                        await context.Channel.SendMessageAsync(embed: EmbedFormats.CreateErrorEmbed("**No permission!**\n" + result.ErrorReason));
                        break;

                    case CommandError.BadArgCount:
                        if (msg.Content.Contains("setbotgame"))
                            break;
                        await context.Channel.SendMessageAsync(embed: EmbedFormats.CreateErrorEmbed($"**Missing Parameters!** Please check command help with `{prefix}help {_commands.Search(context, argPos).Commands[0].Alias}`"));
                        break;

                    case CommandError.Exception:
                        await context.Channel.SendMessageAsync(embed: EmbedFormats.CreateErrorEmbed("An error occurred, please report it to Matcher#0183\n" + result.ErrorReason));
                        break;

                    case CommandError.ParseFailed:
                        await context.Channel.SendMessageAsync(embed: EmbedFormats.CreateErrorEmbed("**Parse Failed!** Please check command syntax"));
                        break;

                    case CommandError.MultipleMatches:
                        await context.Channel.SendMessageAsync(embed: EmbedFormats.CreateErrorEmbed("**Multiple Matches!**"));
                        break;

                    case CommandError.Unsuccessful:
                        await context.Channel.SendMessageAsync(embed: EmbedFormats.CreateErrorEmbed("**Command execution unsuccessful!** Please report this to Matcher#0183"));
                        break;

                    default:
                        await context.Channel.SendMessageAsync(embed: EmbedFormats.CreateErrorEmbed("**An error occurred!** Please report it to Matcher#0183\n" + result.ErrorReason));
                        break;
                }
            }
            //If there are not errors but the command is unknown, send message to server that the command is unknown
            else if (result.Error == CommandError.UnknownCommand)
            {
                await context.Channel.SendMessageAsync(embed: EmbedFormats.CreateErrorEmbed($"**Unknown Command:** Type `{prefix}help` to see available commands."));
            }
            #endregion
        }
    }
}