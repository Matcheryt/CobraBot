namespace CobraBot.Handlers
{
    public class GuildSettings
    {
        public string prefix { get; set; }
        public string roleOnJoin { get; set; }
        public string joinLeaveChannel { get; set; }

        public GuildSettings(string prefix, string roleOnJoin, string joinLeaveChannel)
        {
            this.prefix = prefix;
            this.roleOnJoin = roleOnJoin;
            this.joinLeaveChannel = joinLeaveChannel;
        }
    }
}
