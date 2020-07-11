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
            BsonMapper.Global.Entity<DatabaseEntity>().Id(entity => entity.ObjectId);
            _logger = logger;
        }

        public long AddInitiator(AccessToken token, long state)
        {
            var col = _db.GetCollection<DatabaseEntity>("users");
            var ins = new DatabaseEntity
            {
                Token = token,
                State = state
            };
            

            col.EnsureIndex(it => it.ObjectId);
            return col.Insert(ins).AsInt64;
        }

        public IEnumerable<DatabaseEntity> GetAll()
        {
            var col = _db.GetCollection<DatabaseEntity>("users");
            return col.FindAll();
        }


        public IEnumerable<DatabaseEntity> Find(Expression<Func<DatabaseEntity, bool>> predicate)
        {
            var col = _db.GetCollection<DatabaseEntity>("users");
            return col.Find(predicate);
        }

        public bool Update(DatabaseEntity entity)
        {
            var col = _db.GetCollection<DatabaseEntity>("users");
            return col.Update(entity);
        }

        public int Update(IEnumerable<DatabaseEntity> entities)
        {
            var col = _db.GetCollection<DatabaseEntity>("users");
            return col.Update(entities);
        }
    }
}