using JustLearnIT.Services;
using JustLearnIT.Models;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace JustLearnIT.Security
{
    public static class AuthService
    {
        #region JWT
        public static Task<JwtSecurityToken> AssignToken(UserModel user)
        {
            SymmetricSecurityKey symmetricKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(KeyVaultService.GetSecretByName("JWT--Key")));
            SigningCredentials signingCredentials = new SigningCredentials(symmetricKey, SecurityAlgorithms.HmacSha256Signature);

            List<Claim> claims = new List<Claim>();

            switch (user.Role)
            {
                case Role.USER:
                    claims.Add(new Claim(ClaimTypes.Role, Enum.GetName(typeof(Role), Role.USER)));
                    break;
                case Role.ADMIN:
                    claims.Add(new Claim(ClaimTypes.Role, Enum.GetName(typeof(Role), Role.ADMIN)));
                    break;
            }

            var token = new JwtSecurityToken(
                issuer: "INO",
                audience: user.Login,
                expires: DateTime.Now.AddMinutes(180),
                signingCredentials: signingCredentials,
                claims: claims
                );

            return Task.FromResult(token);
        }

        public static async Task SetJWT(UserModel user, HttpContext context)
        {
            var token = await AssignToken(user);

            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            string tokenString = tokenHandler.WriteToken(token);

            context.Session.SetString("TOKEN", tokenString);
        }

        public static string GetJWTRole(string tokenString)
        {
            if (string.IsNullOrEmpty(tokenString)) return string.Empty;

            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.ReadJwtToken(tokenString);
            return token.Claims.First(x => x.Type.ToString().Equals(ClaimTypes.Role)).Value;
        }

        public static string GetJWTAudience(string tokenString)
        {
            if (string.IsNullOrEmpty(tokenString)) return string.Empty;

            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.ReadJwtToken(tokenString);
            return token.Audiences.First();
        }
        #endregion
    }
}
