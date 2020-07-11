using System.Collections.Generic;
using ShikimoriSharp.Bases;

namespace Anotis.Models.Database
{
    public class DatabaseEntity
    {
        public long ObjectId { get; set; }
        public AccessToken Token { get; set; }
        public List<long> Animes { get; set; }
        public List<long> Mangas { get; set; }
        public long State { get; set; }
    }
}