using System.Threading.Tasks;
using Anotis.Models.Attendance.Shikimori;
using Anotis.Models.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Anotis.Controllers
{
    [ApiController]
    public class ShikimoriController : Controller
    {
        private readonly ShikimoriAttendance _attendance;
        private readonly IConfiguration _configuration;
        private readonly IDatabase _database;
        private readonly ILogger<ShikimoriController> _logger;

        public ShikimoriController(ShikimoriAttendance attendance, IDatabase database,
            ILogger<ShikimoriController> logger, IConfiguration configuration)
        {
            _attendance = attendance;
            _database = database;
            _logger = logger;
            _configuration = configuration;
        }

        [HttpGet("[controller]/auth")]
        public string Auth(long userId)
        {
            _logger.LogInformation($"New user appeared: {userId}");
            return string.Format(_configuration["Shikimori:AuthLinkTemplate"], _configuration["Shikimori:RedirectUrl"],
                userId);
        }

        [HttpGet("[controller]/auth_redirect")]
        public async Task AuthRedirect(string code, long state)
        {
            _logger.LogInformation($"Shikimori_redirect was called: {code}:{state}");
            var token = await _attendance.OAuth(code);
            _database.AddInitiator(token, state);
        }
    }
}