using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace CobraBot.TypeReaders
{
    public class ExtendedRoleTypeReader<T> : RoleTypeReader<IRole>
    {
        public override async Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
        {
            var typeReaderResult = await base.ReadAsync(context, input, services);
            if (typeReaderResult.IsSuccess)
                return typeReaderResult;

            if (!ulong.TryParse(input, out var parseResult) || context is not SocketCommandContext ctx)
                return TypeReaderResult.FromError(CommandError.ObjectNotFound, "Unable to find role!");

            var role = ctx.Guild.GetRole(parseResult);

            return role != null ? TypeReaderResult.FromSuccess(role) : TypeReaderResult.FromError(CommandError.ObjectNotFound, "Unable to find role!");
        }
    }
}
