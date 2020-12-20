using System.Threading.Tasks;
using CobraBot.Services;
using Discord.Commands;

namespace CobraBot.Modules
{
    [RequireContext(ContextType.Guild)]
    [Name("Information Module")]
    public class InfoModule : ModuleBase<SocketCommandContext>
    {
        public InfoService InfoService { get; set; }

        //Sends a message with server information
        [Command("svinfo"), Alias("serverinfo")]
        [Name("Server info"), Summary("Shows current server info")]
        public async Task ServerInfo()
            => await InfoService.ServerInfoAsync(Context);

        //Shows help
        [Command("help")]
        [Name("Help"), Summary("Shows help command")]
        public async Task Help()
            => await InfoService.HelpAsync(Context);

        //Shows help
        [Command("help")]
        [Name("Help"), Summary("Shows information about a command")]
        public async Task Help([Remainder]string command)
            => await InfoService.HelpAsync(Context, command);

        //Shows uptime
        [Command("botinfo"), Alias("info", "binfo", "cobra")]
        [Name("Bot Info"), Summary("Shows Cobra's information")]
        public async Task BotInfo()
            => await InfoService.GetBotInfoGetAsync(Context);
    }
}
