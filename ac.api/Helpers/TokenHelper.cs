using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json;

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
                Expires = DateTime.Now.AddDays(7),
                SigningCredentials = credentials,
                Issuer = issuer,
                IssuedAt = DateTime.Now,
                Audience = audience
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(descriptor);

            return tokenHandler.WriteToken(token);
        }

        public static string JwtTokenGenerator(string data, string key, string issuer, string audience)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.UserData, data),
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

        public static string JwtTokenGenerator<T>(T userInfo, string key, string issuer, string audience)
        {
            var json = JsonSerializer.Serialize(userInfo);

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

        public static JwtSecurityToken ReadToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(token);
            var securityToken = jsonToken as JwtSecurityToken;

            return securityToken;
        }

        public static T ResolveObjectFromToken<T>(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var securityToken = handler.ReadToken(token) as JwtSecurityToken;

            var user64 = securityToken.Claims.First().Value;

            var jsonBytes = Convert.FromBase64String(user64);
            var jsonString = Encoding.ASCII.GetString(jsonBytes);

            var model = JsonSerializer.Deserialize<T>(jsonString);

            return model;
        }

        public static string GenerateTokenFromObject<T>(T data)
        {
            var json = JsonSerializer.Serialize(data);
            var jsonBytes = Encoding.ASCII.GetBytes(json);
            var data64 = Convert.ToBase64String(jsonBytes);

            return data64;
        }
    }
}