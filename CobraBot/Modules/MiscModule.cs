using Discord.Commands;
using System.Threading.Tasks;
using CobraBot.Services;

namespace CobraBot.Modules
{
    [RequireContext(ContextType.Guild)]
    [Name("Misc Module")]
    public class MiscModule : ModuleBase<SocketCommandContext>
    {
        public MiscService MiscService { get; set; }

        //Converts specified value from one currency to another
        [Command("convert"), Alias("conversion", "conv")]
        [Name("Convert"), Summary("Converts value from one currency to another.")]
        public async Task ConvertCurrency(string from, string to, string value)
            => await ReplyAsync(embed: await MiscService.ConvertCurrencyAsync(from, to, value));
    }
}