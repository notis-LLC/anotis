﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using LiteDB;
using Microsoft.Extensions.Logging;
using ShikimoriSharp.AdditionalRequests;
using ShikimoriSharp.Bases;
using ShikimoriSharp.Classes;
using ShikimoriSharp.Enums;

namespace Anotis.Models.Database.LiteDB
{
    public class Lite : IDatabase
    {
        private readonly LiteDatabase _db;
        private readonly ILogger<Lite> _logger;

        public Lite(ILogger<Lite> logger, AnotisConfig config)
        {
            _db = new LiteDatabase(config.LiteDb.ConnectionString);
            BsonMapper.Global.Entity<DatabaseUser>().Id(entity => entity.ObjectId);
            _logger = logger;
        }

        public long AddInitiator(AccessToken token, long state)
        {
            var col = _db.GetCollection<DatabaseUser>("users");
            var ins = new DatabaseUser
            {
                Token = token,
                State = state
            };


            col.EnsureIndex(it => it.ObjectId);
            return col.Insert(ins).AsInt64;
        }

        public IEnumerable<DatabaseUser> GetAllUsers()
        {
            var col = _db.GetCollection<DatabaseUser>("users");
            return col.FindAll();
        }


        public IEnumerable<MangaID> GetAllMangas()
        {
            var col = _db.GetCollection<MangaID>("mangas");
            return col.FindAll();
        }

        public IEnumerable<DatabaseUser> Find(Expression<Func<DatabaseUser, bool>> predicate)
        {
            var col = _db.GetCollection<DatabaseUser>("users");
            return col.Find(predicate);
        }

        public IEnumerable<MangaID> Find(Expression<Func<MangaID, bool>> predicate)
        {
            var col = _db.GetCollection<MangaID>("mangas");
            return col.Find(predicate);
        }

        public IEnumerable<DatabaseExternalLink> FindLinks(Expression<Func<DatabaseExternalLink, bool>> predicate)
        {
            var col = _db.GetCollection<DatabaseExternalLink>("external_links");
            return col.Find(predicate);
        }

        public int CountLinks(Expression<Func<DatabaseExternalLink, bool>> predicate)
        {
            var col = _db.GetCollection<DatabaseExternalLink>("external_links");
            return col.FindOne(predicate).Links.Length;
        }

        public bool Update(DatabaseUser entity)
        {
            var col = _db.GetCollection<DatabaseUser>("users");
            return col.Upsert(entity);
        }

        public bool Delete(DatabaseUser entity)
        {
            var col = _db.GetCollection<DatabaseUser>("users");
            return col.Delete(entity.ObjectId);
        }

        public bool DeleteManga(long id)
        {
            var col = _db.GetCollection<DatabaseExternalLink>("external_links");
            return col.Delete(id);
        }

        public bool Update(DatabaseExternalLink links)
        {
            var col = _db.GetCollection<DatabaseExternalLink>("external_links");
            col.EnsureIndex(x => x.Id);
            return col.Upsert(links);
        }

        public int Update(IEnumerable<DatabaseExternalLink> entity)
        {
            var col = _db.GetCollection<DatabaseExternalLink>("external_links");
            return col.Upsert(entity);
        }

        public int Update(IEnumerable<DatabaseUser> entities)
        {
            var col = _db.GetCollection<DatabaseUser>("users");
            return col.Update(entities);
        }

        public int AddExternalLinks(IEnumerable<DatabaseExternalLink> links)
        {
            var col = _db.GetCollection<DatabaseExternalLink>("external_links");
            col.EnsureIndex(x => x.Id);
            return col.Upsert(links);
        }

        public async Task UpdateLinks(IEnumerable<long> entities, TargetType type,
            Func<TargetType, long, Task<ExternalLinks[]>> updater)
        {
            var links = _db.GetCollection<DatabaseExternalLink>("external_links");
            links.EnsureIndex(x => x.Id);
            foreach (var entity in entities)
                if (!links.Exists(link => link.Id == entity && link.Type == type))
                    links.Upsert(new DatabaseExternalLink
                    {
                        Links = (await updater(type, entity))
                            .Select(it => new ExtendedLink(it, DateTimeOffset.UtcNow.ToUnixTimeSeconds()))
                            .ToArray(),
                        Id = entity,
                        Type = type,
                        UpdatedAt = DateTime.MinValue
                    });
        }

        public async Task UpdateMangaInformation(IEnumerable<long> entities, Func<long, Task<MangaID>> updater)
        {
            var col = _db.GetCollection<MangaID>("mangas");
            var tasks = entities.AsParallel()
                .Where(x => !col.Exists(v => v.Id == x))
                .Select(async it => col.Insert(await updater(it)));
            await Task.WhenAll(tasks);
        }

        public IEnumerable<DatabaseExternalLink> GetAllLinks()
        {
            return _db.GetCollection<DatabaseExternalLink>("external_links").FindAll();
        }
    }
}