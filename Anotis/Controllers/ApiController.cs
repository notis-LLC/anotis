using System;
using System.Linq;
using Anotis.Models.Attendance;
using Anotis.Models.Database;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Anotis.Controllers
{
    [ApiController]
    public class ApiController : Controller
    {
        private readonly IConfiguration _config;
        private readonly ILogger<ApiController> _logger;
        private readonly IDatabase _database;

        public ApiController(IConfiguration config, ILogger<ApiController> logger, IDatabase database)
        {
            _config = config;
            _logger = logger;
            _database = database;
        }
        
        [HttpPost("[controller]/v1/me")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public IActionResult GetMe([FromBody] long id)
        {
            var users = _database.Find(i => i.State == id).ToList();
            var user = users.FirstOrDefault();
            
            if (users.Count > 1) _logger.LogWarning($"Two entries of {id}");
            if (user is null) return NoContent();

            
            var mangas = user.Mangas.AsParallel()
                .Select(it => _database.Find(mangaId => mangaId.Id == it).FirstOrDefault())
                .Where(it => !(it is null))
                .Select(it => $"{it.Russian} из {_database.CountLinks(x => x.Id == it.Id)} источников: {FormUrls(it.Id)}")
                .ToList();

            for (var i = 1; i <= mangas.Count; i++)
            {
                mangas[i - 1] = mangas[i - 1].Insert(0, $"{i}) ");
            }
            
            return Ok($"Telegram id: {user.State}\nShikimori id: {user.ShikimoriId}\nShikimori nickname: {user.ShikimoriNickname}\nLast Update: {user.UpdatedAt}\n" + string.Join(Environment.NewLine, mangas));
        }
        
        [HttpPost("[controller]/v1/start")]
        public string PostStart([FromBody] long id)
        {
            return new UrlResolver(_config).UrlString(id);
        }

        private string FormUrls(long id)
        {
            var links = _database.FindLinks(link => link.Id == id).FirstOrDefault()?.Links;
            if (links is null)
            {
                _logger.LogCritical("what, how?");
                throw new Exception("links was null");
            }

            return string.Join(", ", links.Select(it => $"[{it.Link.Url.GetLeftPart(UriPartial.Authority).Split(it.Link.Url.GetLeftPart(UriPartial.Scheme))[1]}]({it.Link.Url})"));
        }
    }
}