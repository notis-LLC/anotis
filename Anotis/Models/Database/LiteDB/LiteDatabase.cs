using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using LiteDB;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ShikimoriSharp.Bases;

namespace Anotis.Models.Database.LiteDB
{
    public class Lite : IDatabase
    {
        private readonly LiteDatabase _db;
        private readonly ILogger<Lite> _logger;

        public Lite(ILogger<Lite> logger, IConfiguration config)
        {
            _db = new LiteDatabase(config["LiteDB:ConString"]);
            BsonMapper.Global.Entity<DatabaseUser>().Id(entity => entity.ObjectId);
            _logger = logger;
        }

        private BsonValue Update<T>(string name, T value)
        {
            var col = _db.GetCollection<T>(name);
            var res = col.Update(value);
            if (!res) return col.Insert(value);
            
            return true;
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


        public IEnumerable<DatabaseUser> Find(Expression<Func<DatabaseUser, bool>> predicate)
        {
            var col = _db.GetCollection<DatabaseUser>("users");
            return col.Find(predicate);
        }

        public bool Update(DatabaseUser entity)
        {
            var col = _db.GetCollection<DatabaseUser>("users");
            return col.Update(entity);
        }

        public bool Update(DatabaseExternalLink links)
        {
            var col = _db.GetCollection<DatabaseExternalLink>("external_links");
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
            return col.Upsert(links);
        }

        public IEnumerable<DatabaseExternalLink> GetAllLinks()
        {
            return _db.GetCollection<DatabaseExternalLink>("external_links").FindAll();
        }
    }
}