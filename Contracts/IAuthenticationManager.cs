using System.Threading.Tasks;
using Entities.DataTransferObjects;

namespace Contracts
{
    public interface IAuthenticationManager
    {
        Task<bool> ValidateUser(UserForAuthenticatingDto userForAuth);
        Task<string> CreateToken();
    }
}
