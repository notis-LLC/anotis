using System;
using ShikimoriSharp.AdditionalRequests;
using ShikimoriSharp.Enums;

namespace Anotis.Models.Database
{
    public class DatabaseExternalLink
    {
        public ExternalLinks[] Links { get; set; }
        public long Id { get; set; }
        public TargetType Type { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime LastRelease { get; set; } = DateTime.MinValue;
    }
}