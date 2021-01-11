using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace CobraBot.Preconditions
{
    public class CanModerateUser : ParameterPreconditionAttribute
    {
        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, ParameterInfo parameter, object value, IServiceProvider services)
        {
            if (value is not IGuildUser user)
                return PreconditionResult.FromError("Invalid user!");

            if (user.Id == context.User.Id)
                return PreconditionResult.FromError("You can't use this command on yourself!");

            if (user.GuildPermissions.Administrator)
                return PreconditionResult.FromError("The user you're trying to moderate is a mod/admin.");

            var bot = await context.Guild.GetCurrentUserAsync();

            if (bot is SocketGuildUser socketBot && user is SocketGuildUser socketUser)
                if (socketUser.Hierarchy > socketBot.Hierarchy)
                    return PreconditionResult.FromError("Cobra's role isn't high enough to moderate specified user. Move 'Cobra' role up above other roles.");

            return PreconditionResult.FromSuccess();
        }
    }
}
