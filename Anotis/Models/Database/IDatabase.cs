using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ShikimoriSharp.AdditionalRequests;
using ShikimoriSharp.Bases;
using ShikimoriSharp.Classes;
using ShikimoriSharp.Enums;

namespace Anotis.Models.Database
{
    public interface IDatabase
    {
        long AddInitiator(AccessToken token, long state);
        IEnumerable<DatabaseUser> GetAllUsers();
        IEnumerable<DatabaseExternalLink> GetAllLinks();
        IEnumerable<DatabaseUser> Find(Expression<Func<DatabaseUser, bool>> predicate);
        bool Update(DatabaseUser entity);
        bool Delete(DatabaseUser entity);
        bool Update(DatabaseExternalLink entity);
        int Update(IEnumerable<DatabaseExternalLink> entity);
        int Update(IEnumerable<DatabaseUser> entity);
        int AddExternalLinks(IEnumerable<DatabaseExternalLink> links);
        Task UpdateLinks(IEnumerable<long> entities, TargetType type,
            Func<TargetType, long, Task<ExternalLinks[]>> updater);
    }
}