using System.Threading.Tasks;
using CobraBot.Common.EmbedFormats;
using CobraBot.Database;
using Discord;
using Discord.Commands;
using Microsoft.EntityFrameworkCore;

namespace CobraBot.Services.Moderation
{
    public sealed class LookupService
    {
        private readonly BotContext _botContext;

        public LookupService(BotContext botContext)
        {
            _botContext = botContext;
        }

        public async Task LookupCase(SocketCommandContext context, int caseId)
        {
            var modCase = await _botContext.ModCases.AsNoTracking().FirstOrDefaultAsync(x => x.Id == caseId);
            if (modCase == null)
            {
                await context.Channel.SendMessageAsync(
                    embed: CustomFormats.CreateErrorEmbed("Specified case not found!"));
                return;
            }

            string username = context.Client.GetUser(modCase.UserId)?.ToString() ?? modCase.UserName;

            await context.Channel.SendMessageAsync(embed: ModerationFormats.LookupEmbed(modCase, username));
        }
    }
}
