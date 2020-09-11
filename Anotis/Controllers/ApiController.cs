using System;
using System.Linq;
using Anotis.Models.Attendance;
using Anotis.Models.Database;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Anotis.Controllers
{
    public class Text
    {
        [JsonProperty("text")] public string text { get; set; }
    }

    public class Telegram
    {
        [JsonProperty("telegram_id")] public long TelegramId { get; set; }
    }

    [ApiController]
    public class ApiController : Controller
    {
        private readonly AnotisConfig _config;
        private readonly IDatabase _database;
        private readonly ILogger<ApiController> _logger;

        public ApiController(AnotisConfig config, ILogger<ApiController> logger, IDatabase database)
        {
            _config = config;
            _logger = logger;
            _database = database;
        }

        [HttpPost("[controller]/v1/me")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public IActionResult GetMe([FromBody] Telegram id)
        {
            var users = _database.Find(i => i.State == id.TelegramId).ToList();
            var user = users.FirstOrDefault();

            if (users.Count > 1) _logger.LogCritical($"Two entries of {id}");
            if (user is null) return NoContent();


            var mangas = user.Mangas.AsParallel()
                .Select(it => _database.Find(mangaId => mangaId.Id == it).FirstOrDefault())
                .Where(it => !(it is null))
                .Select(it =>
                    $"{it.Russian} из {_database.CountLinks(x => x.Id == it.Id)} источников: {FormUrls(it.Id)}")
                .ToList();

            for (var i = 0; i < mangas.Count; i++) mangas[i] = mangas[i].Insert(0, $"{i + 1}) ");

            return Ok(new Text
            {
                text =
                    $"Telegram id: {user.State}\nShikimori id: {user.ShikimoriId}\nShikimori nickname: {user.ShikimoriNickname}\nLast Update: {user.UpdatedAt}\n" +
                    string.Join(Environment.NewLine, mangas)
            });
        }

        [HttpPost("[controller]/v1/start")]
        public Text PostStart([FromBody] Telegram id)
        {
            return new Text {text = new UrlResolver(_config).UrlString(id.TelegramId)};
        }

        private string FormUrls(long id)
        {
            var links = _database.FindLinks(link => link.Id == id).FirstOrDefault()?.Links;
            if (links is null)
            {
                _logger.LogCritical("what, how? (links was null)");
                throw new Exception("links was null");
            }

            return string.Join(", ",
                links.Select(it =>
                    $"[{it.Link.Url.GetLeftPart(UriPartial.Authority).Split(it.Link.Url.GetLeftPart(UriPartial.Scheme))[1]}]({it.Link.Url})"));
        }
    }
}