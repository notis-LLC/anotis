using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Anotis.Models.Attendance.Shikimori;
using Anotis.Models.Database;
using Microsoft.Extensions.Logging;
using ShikimoriSharp.AdditionalRequests;
using ShikimoriSharp.Enums;

namespace Anotis.Models.BackgroundRefreshing
{
    public class BackgroundUserUpdatesRefresher : TimedHostedService
    {
        private readonly IDatabase _database;
        private readonly ShikimoriAttendance _attendance;
        private readonly ILogger<BackgroundUserUpdatesRefresher> _logger;

        public BackgroundUserUpdatesRefresher(IDatabase database, ShikimoriAttendance attendance, ILogger<BackgroundUserUpdatesRefresher> logger) : base(logger, TimeSpan.FromHours(2))
        {
            _database = database;
            _attendance = attendance;
            _logger = logger;
        }

        protected override async void DoWork(object state)
        {
            _logger.LogInformation("Initiating Full Update");
            var all = _database.GetAllUsers().ToList();
            if (all.Count == 0)
            {
                _logger.LogInformation("No users widePeepoSad");
                return;
            }
            var animes = new HashSet<long>();
            var mangas = new HashSet<long>(); 
            var updated = DateTime.UtcNow;

            foreach (var entity in all)
            {
                entity.Animes = await _attendance.GetAnimeList(entity.Token);
                animes.UnionWith(entity.Animes);
                entity.Mangas = await _attendance.GetMangaList(entity.Token);
                mangas.UnionWith(entity.Mangas);
                entity.UpdatedAt = updated;
            }
            
            var allLinks = _database.GetAllLinks().ToList();
            var newAnimeLinks = await Task.WhenAll(Helper(animes, allLinks.Where(it => it.Type == TargetType.Anime), TargetType.Anime));
            var newMangaLinks =  await Task.WhenAll(Helper(mangas, allLinks.Where(it => it.Type == TargetType.Manga), TargetType.Manga));

            void Insert(IEnumerable<ExternalLinks[]> links, TargetType type)
            {
                _database.Update(links.Select(it => new DatabaseExternalLink
                {
                    Links = it,
                    ShikimoriId = it.First().EntryId,
                    Type = type,
                    UpdatedAt = updated
                }));
            } 
            Insert(newAnimeLinks, TargetType.Anime);
            Insert(newMangaLinks, TargetType.Manga);
            _database.Update(all);
        }

        private IEnumerable<Task<ExternalLinks[]>> Helper(IEnumerable<long> newLinks, IEnumerable<DatabaseExternalLink> source, TargetType type)
        {
            return newLinks
                .AsParallel()
                .Where(it => source.All(x => x.ShikimoriId != it))
                .Select(it => _attendance.GetLinks(type, it));
        }
    }
}