using Discord.Commands;
using Discord;
using System.Threading.Tasks;
using CobraBot.Preconditions;
using CobraBot.Services.Moderation;

namespace CobraBot.Modules
{
    [RequireContext(ContextType.Guild)]
    [Name("Moderation")]
    public class ModerationModule : ModuleBase<SocketCommandContext>
    {
        public ModerationService ModerationService { get; set; }
        public LookupService LookupService { get; set; }

        [RequireBotPermission(GuildPermission.BanMembers)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        [Command("ban")]
        [Name("Ban"), Summary("Bans specified user.")]
        public async Task BanUser([CanModerateUser]IUser user, [Name("prune days")]int pruneDays = 0, [Remainder] string reason = null)
            => await ReplyAsync(embed: await ModerationService.BanAsync(user, pruneDays, reason, Context));


        [RequireBotPermission(GuildPermission.BanMembers)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        [Command("unban")]
        [Name("Unban"), Summary("Unbans specified user.")]
        public async Task UnbanUser(IUser user)
            => await ReplyAsync(embed: await ModerationService.UnbanAsync(user, Context));


        [RequireBotPermission(GuildPermission.KickMembers)]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [Command("kick")]
        [Name("Kick"), Summary("Kicks specified user.")]
        public async Task Kick([CanModerateUser]IUser user, [Remainder] string reason = null)
            => await ReplyAsync(embed: await ModerationService.KickAsync(user, reason, Context));


        [RequireBotPermission(GuildPermission.ManageRoles)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [Command("mute")]
        [Name("Mute"), Summary("Mutes specified user.")]
        public async Task Mute([CanModerateUser]IGuildUser user, [Remainder]string reason = null)
            => await ReplyAsync(embed: await ModerationService.MuteAsync(Context, user, reason));


        [RequireBotPermission(GuildPermission.ManageRoles)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [Name("Unmute"), Summary("Unmutes specified user.")]
        [Command("unmute")]
        public async Task Unmute([CanModerateUser]IGuildUser user)
            => await ReplyAsync(embed: await ModerationService.UnmuteAsync(Context, user));


        [RequireBotPermission(GuildPermission.MuteMembers)]
        [RequireUserPermission(GuildPermission.MuteMembers)]
        [Command("vmute")]
        [Name("Voice mute"), Summary("Voice mutes specified user.")]
        public async Task VoiceMute([CanModerateUser] IGuildUser user, [Remainder]string reason = null)
            => await ReplyAsync(embed: await ModerationService.VoiceMuteAsync(Context, user, reason));


        [RequireBotPermission(GuildPermission.MuteMembers)]
        [RequireUserPermission(GuildPermission.MuteMembers)]
        [Name("Unmute voice"), Summary("Removes voice mute from specified user.")]
        [Command("unvmute")]
        public async Task UnmuteVoice([CanModerateUser] IGuildUser user)
            => await ReplyAsync(embed: await ModerationService.UnmuteVoiceAsync(user));


        [RequireBotPermission(GuildPermission.ManageMessages)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [Command("clean"), Cooldown(1700)]
        [Name("Clean"), Summary("Deletes 'count' messages from the chat.")]
        public async Task CleanMessages(int count)
            => await ModerationService.CleanMessagesAsync(count, Context);


        [RequireBotPermission(GuildPermission.ManageChannels)]
        [RequireUserPermission(GuildPermission.ManageChannels)]
        [Command("slowmode")]
        [Name("Slowmode"), Summary("Sets specified channel slowmode to specified interval. If no interval is specified, the command will reset the channel's slowmode to 0.")]
        public async Task Slowmode(ITextChannel channel, int interval = 0)
            => await ModerationService.SlowmodeAsync(channel, interval, Context);


        [RequireBotPermission(GuildPermission.ManageRoles)]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        [Command("role")]
        [Name("Role"), Summary("Adds/removes role from specified user. Operation + adds role, - removes role.")]
        public async Task UpdateUserRole([CanModerateUser]IGuildUser user, char operation, [Remainder] IRole role)
            => await ReplyAsync(embed: await ModerationService.UpdateRoleAsync(user, operation, role));


        [RequireBotPermission(GuildPermission.BanMembers)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        [Command("lookup")]
        [Name("Lookup"), Summary("Searches for mod case that matches specified case ID")]
        public async Task LookupCase(ulong caseId)
            => await LookupService.LookupCaseAsync(Context, caseId);
    }
}
