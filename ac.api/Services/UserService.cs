using System;
using System.Runtime.Intrinsics.X86;
using System.Globalization;
using System.Threading.Tasks;
using ac.api.Interfaces;
using ac.api.Models.DTO;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using ac.api.Viewmodels;
using ac.api.Constants;

namespace ac.api.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly SignInManager<IdentityUser> signInManager;

        public UserService(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {
            this.signInManager = signInManager;
            this.userManager = userManager;
        }

        public async Task<LoginResultViewmodel> Authenticate(string username, string password)
        {
            var user = await userManager.FindByNameAsync(username);

            // Return null if user not found.
            if (user == null)
            {
                return null;
            }

            // Authenticate the user.
            var signInResult = await signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: false);
            if (!signInResult.Succeeded)
            {
                return null;
            }
            var roles = await userManager.GetRolesAsync(user);
            var userRole = SystemRoles.Admin;
            foreach (var role in roles)
            {
                switch (role)
                {
                    case nameof(SystemRoles.Company):
                        userRole = SystemRoles.Company;
                        break;
                    case nameof(SystemRoles.Client):
                        userRole = SystemRoles.Client;
                        break;
                };
            }

            var result = new LoginResultViewmodel
            {
                UserId = user.Id.ToString(),
                Result = signInResult,
                Role = userRole,
                Username = user.Email
            };

            return result;

            //return token;
        }
    }
}