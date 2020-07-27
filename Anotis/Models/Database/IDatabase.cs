using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using ShikimoriSharp.Bases;

namespace Anotis.Models.Database
{
    public interface IDatabase
    {
        long AddInitiator(AccessToken token, long state);
        IEnumerable<DatabaseUser> GetAllUsers();
        IEnumerable<DatabaseExternalLink> GetAllLinks();
        IEnumerable<DatabaseUser> Find(Expression<Func<DatabaseUser, bool>> predicate);
        bool Update(DatabaseUser entity);
        bool Update(DatabaseExternalLink entity);
        int Update(IEnumerable<DatabaseExternalLink> entity);
        int Update(IEnumerable<DatabaseUser> entity);
        int AddExternalLinks(IEnumerable<DatabaseExternalLink> links);
    }
}