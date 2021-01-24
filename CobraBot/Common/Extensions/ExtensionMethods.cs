using System;
using System.Threading.Tasks;
using CobraBot.Preconditions;
using Discord.Commands;

namespace CobraBot.Common.Extensions
{
    public static class ExtensionMethods
    {
        public static async Task<bool> HasPermissionToExecute(this CommandInfo command, ICommandContext context, IServiceProvider services)
        {
            foreach (var precondition in command.Module.Preconditions)
            {
                if (precondition is Ratelimit || precondition is RequireNsfwAttribute)
                    continue;

                var canExecute = await precondition.CheckPermissionsAsync(context, command, services);

                if (!canExecute.IsSuccess)
                    return false;
            }

            foreach (var precondition in command.Preconditions)
            {
                if (precondition is Ratelimit || precondition is RequireNsfwAttribute)
                    continue;

                var canExecute = await precondition.CheckPermissionsAsync(context, command, services);

                if (!canExecute.IsSuccess)
                    return false;
            }

            return true;
        }
    }
}
