using System.Drawing;
using CobraBot.Services;
using Discord.Commands;
using System.Threading.Tasks;
using Discord;

namespace CobraBot.Modules
{
    [RequireContext(ContextType.Guild)]
    [Name("Utilities")]
    public class MiscModule : ModuleBase<SocketCommandContext>
    {
        public MiscService MiscService { get; set; }

        //Converts specified value from one currency to another
        [Command("convert"), Alias("conversion", "conv")]
        [Name("Convert"), Summary("Converts value from one currency to another.")]
        public async Task ConvertCurrency(string from, string to, string value)
            => await ReplyAsync(embed: await MiscService.ConvertCurrencyAsync(from, to, value));


        //Generate lmgtfy link
        [Command("lmgtfy")]
        [Name("Lmgtfy"), Summary("Creates a lmgtfy link.")]
        public async Task Lmgtfy([Remainder] string textToSearch)
            => await ReplyAsync(MiscService.Lmgtfy(textToSearch));


        //Shows user's avatar and provides links for download in various sizes and formats
        [Command("avatar")]
        [Name("Avatar"), Summary("Shows your avatar and provides links for download.")]
        public async Task Avatar()
            => await ReplyAsync(embed: MiscService.GetAvatar(Context));


        //Shows a color from specified hex color code
        [Command("color")]
        [Name("Color"), Summary("Shows a color from specified hex color code.")]
        public async Task Color(string hexColor)
            => await ReplyAsync(embed: MiscService.GetColorAsync(hexColor));
    }
}