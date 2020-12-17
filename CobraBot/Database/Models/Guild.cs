using System.ComponentModel.DataAnnotations;

namespace CobraBot.Database.Models
{
    public class Guild
    {
        [Key]
        public int Id { get; set; }
        
        public ulong GuildId { get; set; }
        public ulong JoinLeaveChannel { get; set; }
        public string CustomPrefix { get; set; }
        public string RoleOnJoin { get; set; }
    }
}
