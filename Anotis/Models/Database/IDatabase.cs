using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using ShikimoriSharp.Bases;

namespace Anotis.Models.Database
{
    public interface IDatabase
    {
        long AddInitiator(AccessToken token, long state);
        IEnumerable<DatabaseEntity> GetAll();
        IEnumerable<DatabaseEntity> Find(Expression<Func<DatabaseEntity, bool>> predicate);
        bool Update(DatabaseEntity entity);
        int Update(IEnumerable<DatabaseEntity> entity);
    }
}