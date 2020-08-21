using System;
using System.Threading.Tasks;
using Anotis.Models.Attendance.Shikimori;
using Anotis.Models.Database;
using ShikimoriSharp.Enums;

namespace Anotis.Models.Attendance
{
    public class UserReceiver
    {
        private readonly ShikimoriAttendance _attendance;
        private readonly IDatabase _database;

        public UserReceiver(ShikimoriAttendance attendance, IDatabase database)
        {
            _attendance = attendance;
            _database = database;
        }

        public async Task InitiateUser(string code, long state)
        {
            var token = await _attendance.OAuth(code);
            var whoami = _attendance.GetUserId(token);
            var anime = _attendance.GetAnimeList(whoami.Id);
            var manga = _attendance.GetMangaList(whoami.Id);
            await Task.WhenAll(whoami, anime, manga);
            _database.Update(new DatabaseUser
            {
                Token = token,
                State = state,
                Animes = anime.Result,
                Mangas = manga.Result,
                ShikimoriId = whoami.Result.Id,
                ShikimoriNickname = whoami.Result.Nickname,
                UpdatedAt = DateTime.UtcNow
            });

            await Task.WhenAll(
                _database.UpdateMangaInformation(manga.Result, _attendance.GetMangaInformation),
                _database.UpdateLinks(anime.Result, TargetType.Anime, _attendance.GetLinks),
                _database.UpdateLinks(manga.Result, TargetType.Manga, _attendance.GetLinks)
            );
        }
    }
}