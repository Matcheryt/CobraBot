using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Discord.Commands;

namespace CobraBot.Preconditions
{
    /// <summary> Precondition used to prevent user to spam commands. </summary>
    public class CooldownAttribute : PreconditionAttribute
    {
        private readonly ConcurrentDictionary<CooldownInfo, DateTime> _cooldowns = new();
        private TimeSpan CooldownLength { get; }

        /// <summary> Sets the cooldown of the command. </summary>
        /// <param name="milliseconds"> The milliseconds the user will have to wait before being able to execute the command again. </param>
        public CooldownAttribute(double milliseconds)
        {
            CooldownLength = TimeSpan.FromMilliseconds(milliseconds);
        }

        //Checks if a user is on cooldown
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            //Create new cooldown info with context and command information
            var cdInfo = new CooldownInfo(context.User.Id, command.GetHashCode());

            //Try to get an entry
            if (_cooldowns.TryGetValue(cdInfo, out var endsAt))
            {
                //If the current entry is still on cooldown, then return with precondition error
                var difference = endsAt.Subtract(DateTime.Now);
                if (difference.Ticks > 0)
                    return Task.FromResult(PreconditionResult.FromError(""));

                var time = DateTime.Now.Add(CooldownLength);
                _cooldowns.TryUpdate(cdInfo, time, endsAt);
            }
            else _cooldowns.TryAdd(cdInfo, DateTime.Now.Add(CooldownLength));
            
            //If the user isn't on cooldown, return precondition success
            return Task.FromResult(PreconditionResult.FromSuccess());
        }

        //Struct where we save the cooldown information
        public readonly struct CooldownInfo
        {
            //The user ID that this cooldown applies to
            public ulong UserId { get; }

            //The hash code for what command this applies to
            public int CommandHashCode { get; }

            public CooldownInfo(ulong userId, int commandHashCode)
            {
                UserId = userId;
                CommandHashCode = commandHashCode;
            }
        }
    }
}