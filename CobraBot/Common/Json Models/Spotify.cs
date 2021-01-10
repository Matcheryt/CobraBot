using System.Collections.Generic;

namespace CobraBot.Common.Json_Models
{
    public class Track
    {
        public string Name { get; set; }
    }

    public class Item
    {
        public Track Track { get; set; }
    }

    public class Spotify
    {
        public List<Item> Items { get; set; }
    }
}
