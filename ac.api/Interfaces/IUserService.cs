using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace ac.api.Interfaces
{
    public interface IUserService
    {
        Task<SecurityToken> Authenticate(string username, string password, byte[] key, string issuer, string audience);
    }
}