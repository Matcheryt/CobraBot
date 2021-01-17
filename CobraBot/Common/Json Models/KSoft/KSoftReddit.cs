using Newtonsoft.Json;

namespace CobraBot.Common.Json_Models.KSoft
{
    public class KSoftReddit
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("image_url")]
        public string ImageUrl { get; set; }

        [JsonProperty("source")]
        public string Source { get; set; }

        [JsonProperty("subreddit")]
        public string Subreddit { get; set; }

        [JsonProperty("upvotes")]
        public int Upvotes { get; set; }

        [JsonProperty("downvotes")]
        public int Downvotes { get; set; }

        [JsonProperty("comments")]
        public int Comments { get; set; }

        [JsonProperty("created_at")]
        public double CreatedAt { get; set; }

        [JsonProperty("nsfw")]
        public bool Nsfw { get; set; }

        [JsonProperty("author")]
        public string Author { get; set; }

        [JsonProperty("awards")]
        public int Awards { get; set; }
    }
}
