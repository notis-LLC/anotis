using System.Threading.Tasks;
using Anotis.Models;
using Anotis.Models.Attendance;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Anotis.Controllers
{
    [ApiController]
    public class ShikimoriController : Controller
    {
        private readonly AnotisConfig _configuration;
        private readonly ILogger<ShikimoriController> _logger;
        private readonly UserReceiver _receiver;
        private readonly TanserWorker _worker;

        public ShikimoriController(TanserWorker worker, UserReceiver receiver, ILogger<ShikimoriController> logger,
            AnotisConfig configuration)
        {
            _worker = worker;
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
        public async Task<RedirectResult> AuthRedirect(string code, long state)
        {
            _logger.LogInformation($"Shikimori_redirect was called: {code}:{state}");
            await _receiver.InitiateUser(code, state);
            await _worker.SendOk(state);
            return Redirect(_configuration.Services.Tanser.Link);
        }
    }
}