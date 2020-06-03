using System.Threading.Tasks;
using ShikimoriSharp.Bases;

namespace Anotis.Models.SourceWorkers
{
    public interface ISourceWorker
    {
        Task<AccessToken> GenerateToken(string code);

        Task<User> GetUser(AccessToken token);

    }
}