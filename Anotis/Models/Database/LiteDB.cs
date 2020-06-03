﻿using System;
using System.Collections.Generic;
using System.Linq;
using LiteDB;
using ShikimoriSharp.Classes;

namespace Anotis.Models.Database
{
    public class LiteDb : IDatabase
    {
        private const string DbPath = @"Data.db";

        private readonly LiteDatabase _database;
        
        public LiteDb()
        {
            /*
            var mapper = BsonMapper.Global;
            mapper.Entity<User>()
                .DbRef(it => it.Animes, "animes")
                .DbRef(it => it.Mangas, "mangas")
                .Field(x => x.Token, "token")
                .Field(x => x.UserId, "userId");
                */
            _database = new LiteDatabase(DbPath);
            
        }

        
        public void Dispose()
        {
            _database?.Dispose();
        }

        public long InsertUser(User user)
        {
            var col = _database.GetCollection<User>("Users");
            col.EnsureIndex(x => x.UserDbId, true);
            var result = col.Insert(user);
            return result.AsInt64;
        }

        public long UpdateUser(long id, User user)
        {
            var col = _database.GetCollection<User>("Users");
            var val = col.Update(id, user);
            return id;
        }

        public User GetById(long id)
        {
            return _database.GetCollection<User>("Users").FindOne(x => x.UserDbId == id);
        }
    }
}