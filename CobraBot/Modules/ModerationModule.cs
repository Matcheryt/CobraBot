using Discord.Commands;
using Discord;
using System.Threading.Tasks;
using CobraBot.Services;

namespace CobraBot.Modules
{
    [RequireContext(ContextType.Guild)]
    public class ModerationModule : ModuleBase<SocketCommandContext>
    {
        public ModerationService ModerationService { get; set; }

        [RequireBotPermission(GuildPermission.BanMembers)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        [Command("ban")]
        public async Task BanUser(IUser user, int pruneDays = 0, [Remainder] string reason = null)
            => await ReplyAsync(embed: await ModerationService.BanAsync(user, pruneDays, reason, Context));


        //[RequireBotPermission(GuildPermission.BanMembers)]
        //[RequireUserPermission(GuildPermission.BanMembers)]
        //[Command("unban")]
        //public async Task UnbanUser(IUser bannedUser)
        //    => await ReplyAsync(embed: await ModerationService.UnbanAsync(bannedUser, Context));


        [RequireBotPermission(GuildPermission.KickMembers)]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [Command("kick")]
        public async Task Kick(IGuildUser user, [Remainder] string reason = null)
            => await ReplyAsync(embed: await ModerationService.KickAsync(user, reason, Context));


        [RequireBotPermission(GuildPermission.ManageMessages)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [Command("clean", RunMode = RunMode.Async)]
        public async Task CleanMessages(int count = 1)
        {
            var message = await ReplyAsync(embed: await ModerationService.CleanMessagesAsync(count, Context));
            await Task.Delay(2300);
            await message.DeleteAsync();
        }


        [RequireBotPermission(GuildPermission.ManageRoles)]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        [Command("role")]
        public async Task UpdateUserRole(IGuildUser user, char operation, [Remainder]string roleName)
            => await ReplyAsync(embed: await ModerationService.UpdateRoleAsync(user, operation, roleName));


        [RequireUserPermission(GuildPermission.Administrator)]
        [Command("prefix")]
        public async Task SetPrefix(string prefix)
            => await ReplyAsync(embed: await ModerationService.ChangePrefixAsync(prefix, Context));


        [RequireUserPermission(GuildPermission.Administrator)]
        [Command("setwelcome")]
        public async Task SetWelcomeChannel(ITextChannel textChannel)
            => await ReplyAsync(embed: await ModerationService.SetWelcomeChannel(textChannel));


        [RequireUserPermission(GuildPermission.Administrator)]
        [Command("setroleonjoin")]
        public async Task SetRoleOnJoin(string roleName)
            => await ReplyAsync(embed: await ModerationService.SetRoleOnJoin(Context.Guild, roleName));
    }
}
