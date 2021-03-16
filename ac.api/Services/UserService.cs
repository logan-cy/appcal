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

        public async Task<SecurityToken> Authenticate(string username, string password, byte[] key, string issuer, string audience)
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
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.Id.ToString())
                }),
                Expires = DateTime.Now.AddDays(7),
                Issuer = issuer,
                IssuedAt = DateTime.Now,
                Audience = audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha512Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return token;
        }
    }
}