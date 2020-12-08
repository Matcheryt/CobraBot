namespace CobraBot.Handlers
{
    public class GuildSettings
    {
        public string Prefix { get; set; }
        public string RoleOnJoin { get; set; }
        public string JoinLeaveChannel { get; set; }

        public GuildSettings(string prefix, string roleOnJoin, string joinLeaveChannel)
        {
            Prefix = prefix;
            RoleOnJoin = roleOnJoin;
            JoinLeaveChannel = joinLeaveChannel;
        }
    }
}
