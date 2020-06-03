using System.Collections.Generic;
using Anotis.Models.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ShikimoriSharp.Classes;
using User = Anotis.Models.User;

namespace Anotis.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;
        private readonly IDatabase _database;

        public UserController(ILogger<UserController> logger, IDatabase database)
        {
            _logger = logger;
            _database = database;
        }


        [HttpGet("list")]
        // user/list
        public int[] List()
        {
            return new[]
            {
                1, 2, 3, 10
            };
        }

        [HttpGet("find")]
        // user/find?id=23
        public User Find(int id)
        {
            return new User
            {
                UserDbId = id,
                Mangas = new List<MangaID>
                {
                    new MangaID
                    {
                        Russian = "Halo",
                        Id = 2
                    }
                }
            };
    }
    }
}