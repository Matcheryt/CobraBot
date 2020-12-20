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
        [Name("Ban"), Summary("Bans specified user")]
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
        [Name("Kick"), Summary("Kicks specified user")]
        public async Task Kick(IGuildUser user, [Remainder] string reason = null)
            => await ReplyAsync(embed: await ModerationService.KickAsync(user, reason, Context));


        [RequireBotPermission(GuildPermission.ManageRoles)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [Command("mute")]
        [Name("Mute"), Summary("Mutes specified user")]
        public async Task Mute(IGuildUser user)
            => await ReplyAsync(embed: await ModerationService.MuteAsync(user, Context));

        [RequireBotPermission(GuildPermission.ManageRoles)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [Name("Unmute"), Summary("Unmutes specified user")]
        [Command("unmute")]
        public async Task Unmute(IGuildUser user)
            => await ReplyAsync(embed: await ModerationService.UnmuteAsync(user, Context));

        [RequireBotPermission(GuildPermission.MuteMembers)]
        [RequireUserPermission(GuildPermission.MuteMembers)]
        [Command("vmute")]
        [Name("Voice mute"), Summary("Voice mutes specified user")]
        public async Task VMute(IGuildUser user)
            => await ReplyAsync(embed: await ModerationService.VoiceMuteAsync(user, Context));

        [RequireBotPermission(GuildPermission.MuteMembers)]
        [RequireUserPermission(GuildPermission.MuteMembers)]
        [Name("Unmute voice"), Summary("Removes voice mute from specified user")]
        [Command("unvmute")]
        public async Task UnVMute(IGuildUser user)
            => await ReplyAsync(embed: await ModerationService.UnVoiceMuteAsync(user, Context));
        

        [RequireBotPermission(GuildPermission.ManageMessages)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [Command("clean")]
        [Name("Clean"), Summary("Deletes X messages from the chat")]
        public async Task CleanMessages(int count = 1)
            => await ModerationService.CleanMessagesAsync(count, Context);


        [RequireBotPermission(GuildPermission.ManageRoles)]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        [Command("role")]
        [Name("Role"), Summary("Adds/removes role from specified user")]
        public async Task UpdateUserRole(IGuildUser user, char operation, [Remainder]string roleName)
            => await ReplyAsync(embed: await ModerationService.UpdateRoleAsync(user, operation, roleName));


        [RequireUserPermission(GuildPermission.Administrator)]
        [Command("prefix")]
        [Name("Prefix"), Summary("Changes bot prefix for current server")]
        public async Task SetPrefix(string prefix)
            => await ReplyAsync(embed: await ModerationService.ChangePrefixAsync(prefix, Context));


        #region Welcome channel and Role on join
        //Set welcome channel
        [RequireUserPermission(GuildPermission.Administrator)]
        [Command("setwelcome")]
        [Name("Set welcome channel"), Summary("Sets channel where join/left messages are shown")]
        public async Task SetWelcomeChannel(ITextChannel textChannel)
            => await ReplyAsync(embed: await ModerationService.SetWelcomeChannel(textChannel));

        //Reset welcome channel
        [RequireUserPermission(GuildPermission.Administrator)]
        [Command("resetwelcome")]
        [Name("Reset welcome channel"), Summary("Resets channel where join/left messages are shown")]
        public async Task ResetWelcomeChannel()
            => await ReplyAsync(embed: await ModerationService.ResetWelcomeChannel(Context));


        //Set role on join
        [RequireUserPermission(GuildPermission.Administrator)]
        [Command("setroleonjoin")]
        [Name("Set role on join"), Summary("Sets default role that users receive when they join the server")]
        public async Task SetRoleOnJoin(string roleName)
            => await ReplyAsync(embed: await ModerationService.SetRoleOnJoin(Context.Guild, roleName));

        //Reset role on join
        [RequireUserPermission(GuildPermission.Administrator)]
        [Command("resetroleonjoin")]
        [Name("Reset role on join"), Summary("Resets role that users receive when they join the server")]
        public async Task ResetRoleOnJoin()
            => await ReplyAsync(embed: await ModerationService.ResetRoleOnJoin(Context));
#endregion
    }
}
