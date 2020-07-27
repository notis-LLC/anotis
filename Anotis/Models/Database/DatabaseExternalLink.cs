using System;
using ShikimoriSharp.AdditionalRequests;
using ShikimoriSharp.Enums;

namespace Anotis.Models.Database
{
    public class DatabaseExternalLink
    {
        public long Id { get; set; }
        public ExternalLinks[] Links { get; set; }
        public long ShikimoriId { get; set; }
        public TargetType Type { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}