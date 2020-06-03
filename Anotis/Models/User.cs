using System.Collections.Generic;
using ShikimoriSharp.Bases;
using ShikimoriSharp.Classes;

namespace Anotis.Models
{
    public class User
    {
        public long UserDbId { get; set; }
        public Credential UserCredentials { get; set; }
        public AccessToken Token { get; set; }
        public List<MangaID> Mangas { get; set; }
        public List<AnimeID> Animes { get; set; }
    }

    public class Credential
    {
        public Credential(long id, Sources source)
        {
            Id = id;
            Source = source;
        }

        public long Id { get; set; }
        public Sources Source { get; set; }
    }

    public enum Sources
    {
        Shikimori,
    }
}