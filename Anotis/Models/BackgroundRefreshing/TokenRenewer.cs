using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Anotis.Models.Attendance.Shikimori;
using Anotis.Models.Database;
using Microsoft.Extensions.Logging;
using ShikimoriSharp.Bases;

namespace Anotis.Models.BackgroundRefreshing
{
    public class TokenRenewer
    {
        private readonly IDatabase _database;
        private readonly ILogger<TokenRenewer> _logger;
        private readonly ShikimoriAttendance _shiki;

        public TokenRenewer(ShikimoriAttendance shiki, IDatabase database, ILogger<TokenRenewer> logger)
        {
            _shiki = shiki;
            _database = database;
            _logger = logger;
        }

        public async Task<AccessToken> EnsureToken(DatabaseUser user)
        {
            _logger.LogDebug($"Token request for {user.ShikimoriId}");
            var currentTimestamp = DateTimeOffset.UtcNow.AddMinutes(5).ToUnixTimeSeconds();
            if (user.Token.CreatedAt + user.Token.ExpiresIn > currentTimestamp) return user.Token;
            
            _logger.LogInformation($"Token updating: {user.ShikimoriId}");
            try
            {
                user.Token = await _shiki.RefreshOAuth(user.Token);
            }
            catch (Exception e)
            {
                //TODO: What should we do when token's update failed
                //ATM: whatever, we just deleting it 
                _logger.LogCritical($"{e.Message}{Environment.NewLine}{e.StackTrace}");
                _database.Delete(user);
                throw;
            }

            _database.Update(user);
            return user.Token;
        }
    }
}