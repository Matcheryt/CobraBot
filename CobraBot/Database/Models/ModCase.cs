using System;
using System.ComponentModel.DataAnnotations;
using Discord;
using Discord.Commands;

namespace CobraBot.Database.Models
{
    public class ModCase
    {
        public ModCase() { }


        public ModCase(SocketCommandContext context, IUser user, PunishmentType type, string reason) :
            this(context.User, context.Guild.Id, user, type, reason) {}


        public ModCase(IUser modUser, ulong guildId, IUser user, PunishmentType type, string reason)
        {
            GuildId = guildId;
            UserId = user.Id;
            UserName = user.ToString();
            ModId = modUser.Id;
            ModName = modUser.ToString();
            DateTime = DateTimeOffset.UtcNow;
            PunishmentType = type;
            Reason = reason;
        }

        public ModCase(ulong guildId, IUser user, PunishmentType type)
        {
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
