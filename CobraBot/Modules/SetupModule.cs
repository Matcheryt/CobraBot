using CobraBot.Services;
using Discord;
using Discord.Commands;
using System.Threading.Tasks;

namespace CobraBot.Modules
{
    [RequireContext(ContextType.Guild)]
    [RequireUserPermission(GuildPermission.Administrator)]
    [Name("Setup Module")]
    public class SetupModule : ModuleBase<SocketCommandContext>
    {
        public SetupService SetupService { get; set; }

        [Command("setup")]
        [Name("Setup"), Summary("Starts bot setup process.")]
        public async Task Setup()
            => await SetupService.SetupAsync(Context);

        #region Prefix, Welcome Channel, Role on Join, Moderation Channel
        [Command("prefix")]
        [Name("Prefix"), Summary("Changes bot prefix for current server.")]
        public async Task SetPrefix(string prefix)
            => await ReplyAsync(embed: await SetupService.ChangePrefixAsync(prefix, Context));

        //Set welcome channel
        [Command("setwelcome")]
        [Name("Set welcome channel"), Summary("Sets channel where join/left messages are shown.")]
        public async Task SetWelcomeChannel(ITextChannel textChannel)
            => await ReplyAsync(embed: await SetupService.SetWelcomeChannel(textChannel));

        //Reset welcome channel
        [Command("resetwelcome")]
        [Name("Reset welcome channel"), Summary("Resets channel where join/left messages are shown.")]
        public async Task ResetWelcomeChannel()
            => await ReplyAsync(embed: await SetupService.ResetWelcomeChannel(Context));

        //Set role on join
        [Command("setroleonjoin")]
        [Name("Set role on join"), Summary("Sets default role that users receive when they join the server.")]
        public async Task SetRoleOnJoin(string roleName)
            => await ReplyAsync(embed: await SetupService.SetRoleOnJoin(Context.Guild, roleName));

        //Reset role on join
        [Command("resetroleonjoin")]
        [Name("Reset role on join"), Summary("Resets role that users receive when they join the server.")]
        public async Task ResetRoleOnJoin()
            => await ReplyAsync(embed: await SetupService.ResetRoleOnJoin(Context));
        #endregion
    }
}
