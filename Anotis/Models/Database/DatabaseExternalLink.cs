using System;
using ShikimoriSharp.AdditionalRequests;
using ShikimoriSharp.Enums;

namespace Anotis.Models.Database
{
    public class DatabaseExternalLink
    {
        public ExtendedLink[] Links { get; set; }
        public long Id { get; set; }
        public TargetType Type { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}