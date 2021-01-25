#region License
/*CitizenEnforcer - Moderation and logging bot
Copyright(C) 2018-2020 Hawx
https://github.com/Hawxy/CitizenEnforcer

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published
by the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with this program.If not, see http://www.gnu.org/licenses/ */
#endregion

using Discord;
using Discord.Commands;
using System;
using System.ComponentModel.DataAnnotations;

namespace CobraBot.Database.Models
{
    public class ModCase
    {
        public ModCase() { }


        public ModCase(SocketCommandContext context, IUser user, ulong caseId, PunishmentType type, string reason) :
            this(context.User, context.Guild.Id, user, caseId, type, reason)
        { }


        public ModCase(IUser modUser, ulong guildId, IUser user, ulong caseId, PunishmentType type, string reason)
        {
            ModCaseId = caseId;
            GuildId = guildId;
            UserId = user.Id;
            UserName = user.ToString();
            ModId = modUser.Id;
            ModName = modUser.ToString();
            DateTime = DateTimeOffset.UtcNow;
            PunishmentType = type;
            Reason = reason;
        }

        public ModCase(ulong guildId, IUser user, ulong caseId, PunishmentType type)
        {
            ModCaseId = caseId;
            GuildId = guildId;
            UserId = user.Id;
            UserName = user.ToString();
            ModId = 0;
            ModName = "Unknown";
            DateTime = DateTimeOffset.UtcNow;
            PunishmentType = type;
        }

        [Key]
        public int Id { get; set; }

        public ulong ModCaseId { get; set; }
        public ulong GuildId { get; set; }
        public ulong UserId { get; set; }
        public string UserName { get; set; }
        public ulong ModId { get; set; }
        public string ModName { get; set; }
        public DateTimeOffset DateTime { get; set; }
        public PunishmentType PunishmentType { get; set; }
        public string Reason { get; set; }
    }

    public enum PunishmentType
    {
        Mute,
        VMute,
        Kick,
        Ban
    }
}