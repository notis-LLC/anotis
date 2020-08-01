using System;
using Newtonsoft.Json;
using ShikimoriSharp.AdditionalRequests;

namespace Anotis.Models
{
    public class ExtendedLink
    {
        public ExtendedLink()
        {}
        [JsonProperty("links")]
        public ExternalLinks Link { get; set; }
        [JsonProperty("last_update")]
        public DateTime LastUpdate { get; set; }

        public ExtendedLink(ExternalLinks link, DateTime lastUpdate)
        {
            Link = link;
            LastUpdate = lastUpdate;
        }
    }
}