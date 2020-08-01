using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Anotis.Models.Attendance.Shikimori;
using Anotis.Models.Database;
using Microsoft.Extensions.Logging;
using ShikimoriSharp.Enums;

namespace Anotis.Models.BackgroundRefreshing
{
    public class BackgroundNewUserRefresher : TimedHostedService
    {
        private readonly TokenRenewer _renewer;
        private readonly ShikimoriAttendance _attendance;
        private readonly IDatabase _database;
        private readonly ILogger<BackgroundNewUserRefresher> _logger;

        public BackgroundNewUserRefresher(TokenRenewer renewer, ShikimoriAttendance attendance, IDatabase database,
            ILogger<BackgroundNewUserRefresher> logger) : base(logger, TimeSpan.FromMinutes(1))
        {
            _renewer = renewer;
            _attendance = attendance;
            _database = database;
            _logger = logger;
        }

        protected override async void DoWork(object state)
        {
            _logger.LogDebug("New Users Update");
            var res = _database.Find(it => it.Animes == null && it.Mangas == null).ToList();
            if (res.Count == 0)
            {
                _logger.LogInformation("Nothing found, returning");
                return;
            }
            
            var now = DateTime.UtcNow;
            foreach (var entity in res)
            {
                entity.Token = await _renewer.EnsureToken(entity);
                Task[] arr = {
                    _attendance.GetUserId(entity.Token), 
                    _attendance.GetAnimeList(entity.Token),
                    _attendance.GetMangaList(entity.Token)
                };
                await Task.WhenAll(arr);
                try
                {
                    entity.ShikimoriId = ((Task<long>) arr[0]).Result;
                    entity.Animes = ((Task<List<long>>) arr[1]).Result;
                    entity.Mangas = ((Task<List<long>>) arr[2]).Result;
                }
                catch (Exception e)
                {
                    _logger.LogCritical(e.Message);
                    throw;
                }
                
                entity.UpdatedAt = now;
                await Task.WhenAll(
                    _database.UpdateLinks(entity.Mangas, TargetType.Manga, _attendance.GetLinks),
                    _database.UpdateLinks(entity.Animes, TargetType.Anime, _attendance.GetLinks)
                );
            }
            
            _database.Update(res);
        }
        
        
    }
}