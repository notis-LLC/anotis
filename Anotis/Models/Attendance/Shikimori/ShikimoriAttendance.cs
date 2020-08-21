using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ShikimoriSharp;
using ShikimoriSharp.AdditionalRequests;
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

        public Task<MangaID> GetMangaInformation(long id)
        {
            return _client.Mangas.GetById(id);
        }

        public Task<ExternalLinks[]> GetLinks(TargetType type, long id)
        {
            return type == TargetType.Anime ? _client.Animes.GetExternalLinks(id) : _client.Mangas.GetExternalLinks(id);
        }

        public async Task<AccessToken> OAuth(string authCode)
        {
            return await _client.Client.AuthorizationManager.GetAccessToken(authCode);
        }


        public async Task<AccessToken> RefreshOAuth(AccessToken token)
        {
            return await _client.Client.AuthorizationManager.RefreshAccessToken(token);
        }

        private async Task<List<long>> getShit(long id, TargetType type)
        {
            var res = await _client.UserRates.GetUsersRates(new UserRatesSettings
            {
                status = MyList.watching,
                target_type = type,
                user_id = id
            });
            return res.Select(it => it.TargetId).ToList();
        }

        public Task<List<long>> GetAnimeList(long id)
        {
            return getShit(id, TargetType.Anime);
        }

        public Task<List<long>> GetMangaList(long id)
        {
            return getShit(id, TargetType.Manga);
        }

        public Task<UserInfo> GetUserId(AccessToken token)
        {
            return _client.Users.WhoAmI(token);
        }
    }
}