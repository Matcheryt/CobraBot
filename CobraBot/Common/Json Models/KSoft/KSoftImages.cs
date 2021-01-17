using Newtonsoft.Json;

namespace CobraBot.Common.Json_Models.KSoft
{
    public class KSoftImages
    {
        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("snowflake")]
        public string Snowflake { get; set; }

        [JsonProperty("nsfw")]
        public bool Nsfw { get; set; }

        [JsonProperty("tag")]
        public string Tag { get; set; }
    }
}
