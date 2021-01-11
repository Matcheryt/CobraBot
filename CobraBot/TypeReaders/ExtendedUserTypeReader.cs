using Discord;
using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace CobraBot.TypeReaders
{
    public class ExtendedUserTypeReader : UserTypeReader<IUser>
    {
        public override async Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
        {
            var typeReaderResult = await base.ReadAsync(context, input, services);
            if (typeReaderResult.IsSuccess)
                return typeReaderResult;

            if (!ulong.TryParse(input, out var parseResult) || context is not SocketCommandContext ctx)
                return TypeReaderResult.FromError(CommandError.ObjectNotFound, "Unable to find user!");

            var user = await ctx.Client.Rest.GetUserAsync(parseResult);

            return user != null ? TypeReaderResult.FromSuccess(user) : TypeReaderResult.FromError(CommandError.ObjectNotFound, "Unable to find user!");
        }
    }
}
