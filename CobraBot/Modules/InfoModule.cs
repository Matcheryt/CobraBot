using System.Threading.Tasks;
using CobraBot.Common;
using CobraBot.Services;
using Discord;
using Discord.Commands;

namespace CobraBot.Modules
{
    public class InfoModule : ModuleBase<SocketCommandContext>
    {
        public InfoService InfoService { get; set; }

        //Sends a message with server information
        [Command("svinfo"), Alias("info")]
        public async Task ServerInfo()
            => await InfoService.ServerInfoAsync(Context);

        //Shows help
        [Command("help")]
        public async Task Help()
            => await ReplyAsync(embed: EmbedFormats.CreateBasicEmbed("Cobra Commands", "You can check Cobra's commands [here](https://cobra.telmoduarte.me).", Color.DarkGreen));

        //Shows uptime
        [Command("uptime")]
        public async Task Uptime()
            => await InfoService.GetUptimeAsync(Context);
    }
}
