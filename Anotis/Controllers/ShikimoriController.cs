using System.Threading.Tasks;
using Anotis.Models;
using Anotis.Models.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Anotis.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ShikimoriController : ControllerBase
    {
        private readonly ILogger<ShikimoriController> _logger;
        private readonly IControllerService _service;

        private const string ClientId = "nsLe5UGPbFIYLeC_q3Wbm65-wOgB6JOLN46Ukmt_RQM";
        private const string RedirectUrl = "https://cc8b707e.ngrok.io/shikimori/auth";
        public ShikimoriController(ILogger<ShikimoriController> logger, IControllerService service)
        {
            _logger = logger;
            _service = service;
        }
        // shikimori/GenerateAuthLink?code=code&state=214124
        [HttpGet("generate")]
        public string GenerateAuthLink(long id)
        {
            return
                $@"https://shikimori.one/oauth/authorize?client_id={ClientId}&redirect_uri={RedirectUrl}&response_type=code&state={id}&scope=user_rates";
        }
        
        [ProducesResponseType(204)]
        [HttpGet("auth")]
        // shikimori/auth?code=code&state=214124
        public async Task<NoContentResult> Auth(string code, long state)
        {
            await _service.NewUser(Sources.Shikimori, code, state);
            return NoContent();
        }
    }
}