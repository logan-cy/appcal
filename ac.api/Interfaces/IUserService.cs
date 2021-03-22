using System.Threading.Tasks;
using ac.api.Viewmodels;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace ac.api.Interfaces
{
    public interface IUserService
    {
        Task<LoginResultViewmodel> Authenticate(string username, string password);
    }
}