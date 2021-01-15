using CobraBot.Common.EmbedFormats;
using CobraBot.Database;
using Discord.Commands;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace CobraBot.Services.Moderation
{
    public sealed class LookupService
    {
        private readonly BotContext _botContext;

        public LookupService(BotContext botContext)
        {
            _botContext = botContext;
        }

        /// <summary> Searches mod cases for matching case id and returns it. </summary>
        public async Task LookupCaseAsync(SocketCommandContext context, ulong caseId)
        {
            //Try to get the mod case specified
            var modCase = await _botContext.ModCases.AsNoTracking().FirstOrDefaultAsync(x => x.ModCaseId == caseId && x.GuildId == context.Guild.Id);

            //If there isn't any mod case for specified id, then return
            if (modCase == null)
            {
                await context.Channel.SendMessageAsync(
                    embed: CustomFormats.CreateErrorEmbed("Specified case not found!"));
                return;
            }

            //Try to get the latest user username as the one in mod case can be outdated.
            //If we can't get the username for some reason, use the one in the mod case
            string username = context.Client.GetUser(modCase.UserId)?.ToString() ?? modCase.UserName;
            string modUsername = context.Client.GetUser(modCase.ModId)?.ToString() ?? modCase.ModName;

            //Send the mod case
            await context.Channel.SendMessageAsync(embed: ModerationFormats.LookupEmbed(modCase, username, modUsername));
        }
    }
}
