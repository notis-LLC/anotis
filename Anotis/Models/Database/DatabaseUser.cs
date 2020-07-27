using System;
using System.Collections.Generic;
using ShikimoriSharp.Bases;

namespace Anotis.Models.Database
{
    public class DatabaseUser
    {
        public long ObjectId { get; set; }
        public long ShikimoriId { get; set; }
        public AccessToken Token { get; set; }
        public List<long> Animes { get; set; }
        public List<long> Mangas { get; set; }
        public long State { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}