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

using Newtonsoft.Json;
using System.Collections.Generic;

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
