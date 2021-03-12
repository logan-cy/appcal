using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace ac.api.Helpers
{
    public static class TokenHelper
    {
        public static async Task<string> JwtTokenGenerator(IdentityUser userInfo, UserManager<IdentityUser> userManager, string key, string issuer, string audience)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userInfo.Id),
                new Claim(ClaimTypes.Name, userInfo.UserName)
            };

            var roles = await userManager.GetRolesAsync(userInfo);
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha512Signature);

            var descriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = credentials,
                Issuer = issuer,
                IssuedAt = DateTime.Now,
                Audience = audience
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(descriptor);

            return tokenHandler.WriteToken(token);
        }

        public static string JwtTokenGenerator<T>(T userInfo, string key, string issuer, string audience)
        {
            var json = JsonConvert.SerializeObject(userInfo);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.UserData, json),
            };

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha512Signature);

            var descriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = credentials,
                Issuer = issuer,
                IssuedAt = DateTime.Now,
                Audience = audience
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(descriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}