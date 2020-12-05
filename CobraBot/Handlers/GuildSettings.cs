namespace CobraBot.Handlers
{
    public class GuildSettings
    {
        public string Prefix { get; set; }
        public string RoleOnJoin { get; set; }
        public string JoinLeaveChannel { get; set; }

        public GuildSettings(string prefix, string roleOnJoin, string joinLeaveChannel)
        {
            this.Prefix = prefix;
            this.RoleOnJoin = roleOnJoin;
            this.JoinLeaveChannel = joinLeaveChannel;
        }
    }
}
