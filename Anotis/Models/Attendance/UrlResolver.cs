using Microsoft.Extensions.Configuration;

namespace Anotis.Models.Attendance
{
    public class UrlResolver
    {
        private readonly IConfiguration _config;

        public UrlResolver(IConfiguration config)
        {
            _config = config;
        }

        public string UrlString(long userId)
        {
            return string.Format(_config["Shikimori:AuthLinkTemplate"], _config["Shikimori:RedirectUrl"], userId);
        }
    }
}