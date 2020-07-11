using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ShikimoriSharp;
using ShikimoriSharp.Bases;
using ShikimoriSharp.Classes;
using ShikimoriSharp.Settings;

namespace Anotis.Models.Attendance.Shikimori
{
    public class ShikimoriAttendance : IAttendance
    {
        private readonly ILogger<ShikimoriAttendance> _logger;
        private readonly ShikimoriClient _client;

        public ShikimoriAttendance(ILogger<ShikimoriAttendance> logger, ShikimoriClient client)
        {
            _logger = logger;
            _client = client;
        }
        
        public async Task<AccessToken> OAuth(string authCode)
        {
            return await _client.Client.AuthorizationManager.GetAccessToken(authCode);
        }

        public async Task<UserRate[]> GetRates(AccessToken token)
        {
            return await _client.UserRates.GetUsersRates(new UserRatesSettings
            {
                user_id = (await _client.Users.WhoAmI(token)).Id,

            });
        }
        
        
        public async Task<AccessToken> RefreshOAuth(AccessToken token)
        {
            return await _client.Client.AuthorizationManager.RefreshAccessToken(token);
        }
    }
}