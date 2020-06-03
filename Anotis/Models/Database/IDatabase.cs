using System;
using System.Collections.Generic;
using ShikimoriSharp.Classes;
using static ShikimoriSharp.Information.Mangas;

namespace Anotis.Models.Database
{
    public interface IDatabase : IDisposable
    {
        long InsertUser(User user);
        long UpdateUser(long id, User user);
        User GetById(long id);
    }
}