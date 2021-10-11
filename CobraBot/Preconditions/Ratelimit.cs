/*
    Multi-purpose Discord Bot named Cobra
    Copyright (C) 2021 Telmo Duarte <contact@telmoduarte.me>

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License as published
    by the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Affero General Public License for more details.

    You should have received a copy of the GNU Affero General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>. 
*/

// Original code by Joe4evr Discord.Addons

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace CobraBot.Preconditions
{
    /// <summary>
    ///     Sets how often a user is allowed to use this command
    ///     or any command in this module.
    /// </summary>
    /// <remarks>
    ///     <note type="warning">
    ///         This is backed by an in-memory collection
    ///         and will not persist with restarts.
    ///     </note>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false)]
    public sealed class Ratelimit : PreconditionAttribute
    {
        private readonly bool _applyPerGuild;

        private readonly uint _invokeLimit;
        private readonly TimeSpan _invokeLimitPeriod;
        private readonly Dictionary<(ulong, ulong?), CommandTimeout> _invokeTracker = new();
        private readonly bool _noLimitForAdmins;
        private readonly bool _noLimitInDMs;

        /// <summary>
        ///     Sets how often a user is allowed to use this command.
        /// </summary>
        /// <param name="times">
        ///     The number of times a user may use the command within a certain period.
        /// </param>
        /// <param name="period">
        ///     The amount of time since first invoke a user has until the limit is lifted.
        /// </param>
        /// <param name="measure">
        ///     The scale in which the <paramref name="period" /> parameter should be measured.
        /// </param>
        /// <param name="flags">
        ///     Flags to set behavior of the ratelimit.
        /// </param>
        public Ratelimit(
            uint times, double period, Measure measure,
            RatelimitFlags flags = RatelimitFlags.None)
        {
            _invokeLimit = times;
            _noLimitInDMs = (flags & RatelimitFlags.NoLimitInDMs) == RatelimitFlags.NoLimitInDMs;
            _noLimitForAdmins = (flags & RatelimitFlags.NoLimitForAdmins) == RatelimitFlags.NoLimitForAdmins;
            _applyPerGuild = (flags & RatelimitFlags.ApplyPerGuild) == RatelimitFlags.ApplyPerGuild;

            _invokeLimitPeriod = measure switch
            {
                Measure.Days => TimeSpan.FromDays(period),
                Measure.Hours => TimeSpan.FromHours(period),
                Measure.Minutes => TimeSpan.FromMinutes(period),
                Measure.Seconds => TimeSpan.FromSeconds(period),
                Measure.Milliseconds => TimeSpan.FromMilliseconds(period),
                _ => throw new ArgumentOutOfRangeException(nameof(period),
                    "Argument was not within the valid range.")
            };
        }

        /// <summary>
        ///     Sets how often a user is allowed to use this command.
        /// </summary>
        /// <param name="times">
        ///     The number of times a user may use the command within a certain period.
        /// </param>
        /// <param name="period">
        ///     The amount of time since first invoke a user has until the limit is lifted.
        /// </param>
        /// <param name="flags">
        ///     Flags to set bahavior of the ratelimit.
        /// </param>
        /// <remarks>
        ///     <note type="warning">
        ///         This is a convinience constructor overload for use with the dynamic
        ///         command builders, but not with the Class &amp; Method-style commands.
        ///     </note>
        /// </remarks>
        public Ratelimit(
            uint times, TimeSpan period,
            RatelimitFlags flags = RatelimitFlags.None)
        {
            _invokeLimit = times;
            _noLimitInDMs = (flags & RatelimitFlags.NoLimitInDMs) == RatelimitFlags.NoLimitInDMs;
            _noLimitForAdmins = (flags & RatelimitFlags.NoLimitForAdmins) == RatelimitFlags.NoLimitForAdmins;
            _applyPerGuild = (flags & RatelimitFlags.ApplyPerGuild) == RatelimitFlags.ApplyPerGuild;

            _invokeLimitPeriod = period;
        }

        public override string ErrorMessage { get; set; }

        /// <inheritdoc />
        public override Task<PreconditionResult> CheckPermissionsAsync(
            ICommandContext context, CommandInfo _, IServiceProvider __)
        {
            if (_noLimitInDMs && context.Channel is IPrivateChannel)
                return Task.FromResult(PreconditionResult.FromSuccess());

            if (_noLimitForAdmins && context.User is IGuildUser { GuildPermissions: { Administrator: true } })
                return Task.FromResult(PreconditionResult.FromSuccess());

            var now = DateTime.UtcNow;
            var key = _applyPerGuild ? (context.User.Id, context.Guild?.Id) : (context.User.Id, null);

            var timeout = _invokeTracker.TryGetValue(key, out var t)
                          && now - t.FirstInvoke < _invokeLimitPeriod
                ? t
                : new CommandTimeout(now);

            timeout.TimesInvoked++;

            if (timeout.TimesInvoked > _invokeLimit)
                return Task.FromResult(PreconditionResult.FromError(
                    ErrorMessage ?? ""));

            _invokeTracker[key] = timeout;
            return Task.FromResult(PreconditionResult.FromSuccess());
        }

        private sealed class CommandTimeout
        {
            public CommandTimeout(DateTime timeStarted)
            {
                FirstInvoke = timeStarted;
            }

            public uint TimesInvoked { get; set; }
            public DateTime FirstInvoke { get; }
        }
    }

    /// <summary>
    ///     Determines the scale of the period parameter.
    /// </summary>
    public enum Measure
    {
        /// <summary>
        ///     Period is measured in days.
        /// </summary>
        Days,

        /// <summary>
        ///     Period is measured in hours.
        /// </summary>
        Hours,

        /// <summary>
        ///     Period is measured in minutes.
        /// </summary>
        Minutes,

        /// <summary>
        ///     Period is measured in seconds.
        /// </summary>
        Seconds,

        /// <summary>
        ///     Period is measured in milliseconds.
        /// </summary>
        Milliseconds
    }

    /// <summary>
    ///     Determines the behavior of the <see cref="Ratelimit" />.
    /// </summary>
    [Flags]
    public enum RatelimitFlags
    {
        /// <summary>
        ///     Set none of the flags.
        /// </summary>
        None = 0,

        /// <summary>
        ///     Set whether or not there is no limit to the command in DMs.
        /// </summary>
        NoLimitInDMs = 1 << 0,

        /// <summary>
        ///     Set whether or not there is no limit to the command for guild admins.
        /// </summary>
        NoLimitForAdmins = 1 << 1,

        /// <summary>
        ///     Set whether or not to apply a limit per guild.
        /// </summary>
        ApplyPerGuild = 1 << 2
    }
}