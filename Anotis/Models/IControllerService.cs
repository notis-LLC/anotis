using System.Threading.Tasks;

namespace Anotis.Models
{
    public interface IControllerService
    {
        Task NewUser(Sources source, string code, long state);
    }
}