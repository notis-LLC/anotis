using System.Threading.Tasks;
using Anotis.Models.Attendance;
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
        private readonly IConfiguration _configuration;
        private readonly UserReceiver _receiver;
        private readonly ILogger<ShikimoriController> _logger;

        public ShikimoriController(UserReceiver receiver, ILogger<ShikimoriController> logger, IConfiguration configuration)
        {
            _receiver = receiver;
            _logger = logger;
            _configuration = configuration;
        }

        [HttpGet("[controller]/auth")]
        public string Auth(long userId)
        {
            _logger.LogInformation($"New user appeared: {userId}");
            return new UrlResolver(_configuration).UrlString(userId);
        }

        [HttpGet("[controller]/auth_redirect")]
        public async Task<string> AuthRedirect(string code, long state)
        {
            _logger.LogInformation($"Shikimori_redirect was called: {code}:{state}");
            await _receiver.InitiateUser(code, state);
            return "Everything is fine :)";
        }
    }
}