using Discord.Commands;
using Discord;
using System.Threading.Tasks;
using CobraBot.Services;

namespace CobraBot.Modules
{
    [RequireContext(ContextType.Guild)]
    [Name("Moderation Module")]
    public class ModerationModule : ModuleBase<SocketCommandContext>
    {
        public ModerationService ModerationService { get; set; }

        [RequireBotPermission(GuildPermission.BanMembers)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        [Command("ban")]
        [Name("Ban"), Summary("Bans specified user.")]
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
        [Name("Kick"), Summary("Kicks specified user.")]
        public async Task Kick(IGuildUser user, [Remainder] string reason = null)
            => await ReplyAsync(embed: await ModerationService.KickAsync(user, reason, Context));


        [RequireBotPermission(GuildPermission.ManageRoles)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [Command("mute")]
        [Name("Mute"), Summary("Mutes specified user.")]
        public async Task Mute(IGuildUser user)
            => await ReplyAsync(embed: await ModerationService.MuteAsync(user, Context));

        [RequireBotPermission(GuildPermission.ManageRoles)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [Name("Unmute"), Summary("Unmutes specified user.")]
        [Command("unmute")]
        public async Task Unmute(IGuildUser user)
            => await ReplyAsync(embed: await ModerationService.UnmuteAsync(user, Context));

        [RequireBotPermission(GuildPermission.MuteMembers)]
        [RequireUserPermission(GuildPermission.MuteMembers)]
        [Command("vmute")]
        [Name("Voice mute"), Summary("Voice mutes specified user.")]
        public async Task VoiceMute(IGuildUser user)
            => await ReplyAsync(embed: await ModerationService.VoiceMuteAsync(user, Context));

        [RequireBotPermission(GuildPermission.MuteMembers)]
        [RequireUserPermission(GuildPermission.MuteMembers)]
        [Name("Unmute voice"), Summary("Removes voice mute from specified user.")]
        [Command("unvmute")]
        public async Task UnmuteVoice(IGuildUser user)
            => await ReplyAsync(embed: await ModerationService.UnmuteVoiceAsync(user, Context));
        

        [RequireBotPermission(GuildPermission.ManageMessages)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [Command("clean")]
        [Name("Clean"), Summary("Deletes count messages from the chat.")]
        public async Task CleanMessages(int count = 1)
            => await ModerationService.CleanMessagesAsync(count, Context);


        [RequireBotPermission(GuildPermission.ManageRoles)]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        [Command("role")]
        [Name("Role"), Summary("Adds/removes role from specified user. Operation + adds role, - removes role.")]
        public async Task UpdateUserRole(IGuildUser user, char operation, [Remainder]string roleName)
            => await ReplyAsync(embed: await ModerationService.UpdateRoleAsync(user, operation, roleName));


        [RequireBotPermission(GuildPermission.ManageChannels)]
        [RequireUserPermission(GuildPermission.ManageChannels)]
        [Command("slowmode")]
        [Name("Slowmode"), Summary("Sets specified channel slowmode to specified interval")]
        public async Task Slowmode(ITextChannel channel, int interval = 0)
            => await ModerationService.SlowmodeAsync(channel, interval, Context);
    }
}
