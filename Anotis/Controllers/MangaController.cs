using System.Collections.Generic;
using System.Linq;
using Anotis.Models.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Anotis.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MangaController : ControllerBase
    {
        private readonly ILogger<MangaController> _logger;
        private readonly IDatabase _database;

        public MangaController(ILogger<MangaController> logger, IDatabase database)
        {
            _logger = logger;
            _database = database;
        }

        [HttpGet("updates")]
        // manga/updates?limit=23
        public IEnumerable<int> MangaUpdates(int limit)
        {
            return Enumerable.Range(0, limit);
        }
        
        [HttpGet("user/updates")]
        // manga/user/updates?id=23
        public int[] UserUpdates(int id)
        {
            return new[]
            {
                42, 1, 23, 4, 1
            };
        }
    }
}