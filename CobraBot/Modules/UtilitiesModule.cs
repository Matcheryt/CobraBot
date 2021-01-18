using CobraBot.Services;
using Discord.Commands;
using System.Threading.Tasks;
using CobraBot.Preconditions;

namespace CobraBot.Modules
{
    [RequireContext(ContextType.Guild)]
    [Name("Utilities")]
    public class UtilitiesModule : ModuleBase<SocketCommandContext>
    {
        public UtilitiesService UtilitiesService { get; set; }

        //Random number between minVal and maxVal
        [Command("random")]
        [Name("Random"), Summary("Prints random number between two specified numbers.")]
        public async Task RandomNumber([Name("first number")] int minVal = 0, [Name("last number")] int maxVal = 100)
            => await ReplyAsync(embed: UtilitiesService.RandomNumberAsync(minVal, maxVal));


        //Poll command
        [Command("poll"), Ratelimit(1, 1, Measure.Seconds)]
        [Name("Poll"), Summary("Creates a poll with specified question and choices. Make sure to write the parameters between \"quotation marks\"")]
        public async Task Poll(string question, [Name("choice 1")] string choice1, [Name("choice 2")] string choice2)
            => await UtilitiesService.CreatePollAsync(question, choice1, choice2, Context);


        //Converts specified value from one currency to another
        [Command("convert"), Alias("conversion", "conv"), Ratelimit(1, 1700, Measure.Milliseconds)]
        [Name("Convert"), Summary("Converts value from one currency to another.")]
        public async Task ConvertCurrency(string from, string to, string value)
            => await ReplyAsync(embed: await UtilitiesService.ConvertCurrencyAsync(from, to, value));


        //Generate lmgtfy link
        [Command("lmgtfy")]
        [Name("Lmgtfy"), Summary("Creates a lmgtfy link.")]
        public async Task Lmgtfy([Name("text to search")][Remainder] string textToSearch)
            => await ReplyAsync(UtilitiesService.Lmgtfy(textToSearch));


        //Shows user's avatar and provides links for download in various sizes and formats
        [Command("avatar")]
        [Name("Avatar"), Summary("Shows your avatar and provides links for download.")]
        public async Task Avatar()
            => await ReplyAsync(embed: UtilitiesService.GetAvatar(Context));


        //Shows a color from specified hex color code
        [Command("color")]
        [Name("Color"), Summary("Shows a color from specified hex color code.")]
        public async Task Color(string hexColor)
            => await ReplyAsync(embed: UtilitiesService.GetColorAsync(hexColor));
    }
}