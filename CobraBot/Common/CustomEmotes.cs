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

using Discord;

namespace CobraBot.Common
{
    /// <summary> Custom emotes used to beautify messages </summary>
    public static class CustomEmotes
    {
        public static readonly IEmote MetascoreEmote = Emote.Parse("<:metascore:792875985019732038>");
        public static readonly IEmote RottenTomatoesEmote = Emote.Parse("<:rottenTomatoes:792876915086196756>");
        public static readonly IEmote ImdbEmote = Emote.Parse("<:imdb:792877767947059230>");
        public static readonly IEmote SteamEmote = Emote.Parse("<:steam:792883851826823218>");
        public static readonly IEmote CovidEmote = Emote.Parse("<:covidCases:799666630967427082>");
    }
}
