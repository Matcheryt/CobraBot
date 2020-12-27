using System.Collections.Generic;
using Newtonsoft.Json;

namespace CobraBot.Common.Json_Models
{
    public class Steam
    {
        [JsonProperty("response")]
        public Response Response { get; set; }
    }

    public class Player
    {
        [JsonProperty("steamid")]
        public string Steamid { get; set; }

        [JsonProperty("communityvisibilitystate")]
        public int Communityvisibilitystate { get; set; }

        [JsonProperty("profilestate")]
        public int Profilestate { get; set; }

        [JsonProperty("personaname")]
        public string Personaname { get; set; }

        [JsonProperty("commentpermission")]
        public int Commentpermission { get; set; }

        [JsonProperty("profileurl")]
        public string Profileurl { get; set; }

        [JsonProperty("avatar")]
        public string Avatar { get; set; }

        [JsonProperty("avatarmedium")]
        public string Avatarmedium { get; set; }

        [JsonProperty("avatarfull")]
        public string Avatarfull { get; set; }

        [JsonProperty("avatarhash")]
        public string Avatarhash { get; set; }

        [JsonProperty("lastlogoff")]
        public int Lastlogoff { get; set; }

        [JsonProperty("personastate")]
        public int Personastate { get; set; }

        [JsonProperty("realname")]
        public string Realname { get; set; }

        [JsonProperty("primaryclanid")]
        public string Primaryclanid { get; set; }

        [JsonProperty("timecreated")]
        public int Timecreated { get; set; }

        [JsonProperty("personastateflags")]
        public int Personastateflags { get; set; }

        [JsonProperty("loccountrycode")]
        public string Loccountrycode { get; set; }
    }

    public class Response
    {
        [JsonProperty("players")]
        public List<Player> Players { get; set; }
    }
}
