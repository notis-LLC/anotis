using Flurl;

namespace Anotis.Models.BackgroundRefreshing
{
    using System;
    using Newtonsoft.Json;

    public class MangaUpdatedCluster
    {
        public string Url { get; set; }
        [JsonProperty("mangas")]
        public UpdatedManga[] Mangas { get; set; }

        [JsonProperty("total")]
        public long Total { get; set; }
    }

    public class UpdatedManga
    {
        [JsonProperty("tome")]
        public long Tome { get; set; }

        [JsonProperty("number")]
        public long Number { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("date")]
        public DateTime Date { get; set; }

        [JsonProperty("href")]
        public Uri Href { get; set; }
    }
}