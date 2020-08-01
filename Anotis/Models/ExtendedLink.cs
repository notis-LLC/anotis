using System;
using Newtonsoft.Json;
using ShikimoriSharp.AdditionalRequests;

namespace Anotis.Models
{
    public class ExtendedLink
    {
        public ExtendedLink()
        {
        }

        public ExtendedLink(ExternalLinks link, DateTime lastUpdate)
        {
            Link = link;
            LastUpdate = lastUpdate;
        }

        [JsonProperty("links")] public ExternalLinks Link { get; set; }

        [JsonProperty("last_update")] public DateTime LastUpdate { get; set; }
    }
}