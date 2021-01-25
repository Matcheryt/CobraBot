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

using System.Collections.Generic;
using Newtonsoft.Json;

namespace CobraBot.Common.Json_Models.KSoft
{
    public class KSoftLyrics
    {
        [JsonProperty("data")]
        public List<Data> Data { get; set; }
    }

    public class Spotify
    {
        [JsonProperty("artists")]
        public List<string> Artists { get; set; }

        [JsonProperty("track")]
        public string Track { get; set; }

        [JsonProperty("album")]
        public string Album { get; set; }
    }

    public class Deezer
    {
        [JsonProperty("artists")]
        public List<string> Artists { get; set; }

        [JsonProperty("track")]
        public string Track { get; set; }

        [JsonProperty("album")]
        public string Album { get; set; }
    }

    public class Artist
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("is_primary")]
        public bool IsPrimary { get; set; }

        [JsonProperty("id")]
        public int Id { get; set; }
    }

    public class Meta
    {
        [JsonProperty("spotify")]
        public Spotify Spotify { get; set; }

        [JsonProperty("deezer")]
        public Deezer Deezer { get; set; }

        [JsonProperty("artists")]
        public List<Artist> Artists { get; set; }
    }

    public class Data
    {
        [JsonProperty("artist")]
        public string Artist { get; set; }

        [JsonProperty("artist_id")]
        public int ArtistId { get; set; }

        [JsonProperty("album")]
        public string Album { get; set; }

        [JsonProperty("album_ids")]
        public string AlbumIds { get; set; }

        [JsonProperty("album_year")]
        public string AlbumYear { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("lyrics")]
        public string Lyrics { get; set; }

        [JsonProperty("album_art")]
        public string AlbumArt { get; set; }

        [JsonProperty("meta")]
        public Meta Meta { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }
    }
}
