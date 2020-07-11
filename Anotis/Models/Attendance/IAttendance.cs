using System.Threading.Tasks;
using ShikimoriSharp.Bases;

namespace Anotis.Models.Attendance
{
    public interface IAttendance
    {
        public Task<AccessToken> OAuth(string authCode);
        public Task<AccessToken> RefreshOAuth(AccessToken token);
    }
}