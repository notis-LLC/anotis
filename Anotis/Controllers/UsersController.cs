using System.Collections.Generic;
using Anotis.Models.Database;
using Microsoft.AspNetCore.Mvc;

namespace Anotis.Controllers
{
    [ApiController]
    public class UsersController : Controller
    {
        private readonly IDatabase _database;

        public UsersController(IDatabase database)
        {
            _database = database;
        }
        [HttpGet("[controller]/users/get_all")]
        public IEnumerable<DatabaseEntity> GetAll()
        {
            return _database.GetAll();
        }
    }
}