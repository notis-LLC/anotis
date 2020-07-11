using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ShikimoriSharp;
using ShikimoriSharp.Bases;
using ShikimoriSharp.Classes;
using ShikimoriSharp.Enums;
using ShikimoriSharp.Settings;

namespace Anotis.Models.Attendance.Shikimori
{
    public class ShikimoriAttendance
    {
        private readonly ShikimoriClient _client;
        private readonly ILogger<ShikimoriAttendance> _logger;

        public ShikimoriAttendance(ILogger<ShikimoriAttendance> logger, ShikimoriClient client)
        {
            _logger = logger;
            _client = client;
        }

        public async Task<AccessToken> OAuth(string authCode)
        {
            return await _client.Client.AuthorizationManager.GetAccessToken(authCode);
        }


        public async Task<AccessToken> RefreshOAuth(AccessToken token)
        {
            return await _client.Client.AuthorizationManager.RefreshAccessToken(token);
        }

        public async Task<List<long>> GetAnimeList(AccessToken token)
        {
            var animes = new List<Anime>();
            for (var i = 1; ; i++)
            {
                var page = await _client.Animes.GetAnime(new AnimeRequestSettings
                {
                    limit = 50,
                    page = i,
                    status = "ongoing",
                    mylist = MyList.watching
                }, token);
                animes.AddRange(page);
                if(page.Length < 50) break;
            }

            return animes.Select(it => it.Id).ToList();
        }
        public async Task<List<long>> GetMangaList(AccessToken token)
        {
            var mangas = new List<Manga>();
            for (var i = 1; ; i++)
            {
                var page = await _client.Mangas.GetBySearch(new MangaRequestSettings
                {
                    limit = 50,
                    page = i,
                    status = "ongoing",
                    mylist = MyList.watching
                }, token);
                mangas.AddRange(page);
                if(page.Length < 50) break;
            }

            return mangas.Select(it => it.Id).ToList();
        }

        public async Task<long> GetUserId(AccessToken token)
        {
            return (await _client.Users.WhoAmI(token)).Id;
        }
    }
}