using System;
using Newtonsoft.Json;

namespace Anotis.Models.BackgroundRefreshing
{
    public class MangaUpdatedCluster
    {
        public long Id { get; set; }
        public string Url { get; set; }

        [JsonProperty("mangas")] public UpdatedManga[] Mangas { get; set; }

        [JsonProperty("total")] public long Total { get; set; }
    }

    public class UpdatedManga
    {
        [JsonProperty("tome")] public long Tome { get; set; }

        [JsonProperty("number")] public long Number { get; set; }

        [JsonProperty("name")] public string Name { get; set; }

        [JsonProperty("date")] public long Date { get; set; }

        [JsonProperty("href")] public Uri Href { get; set; }
    }
}