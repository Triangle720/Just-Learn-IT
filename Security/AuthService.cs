using JustLearnIT.Services;
using JustLearnIT.Models;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

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
                audience: user.Login.ToString(),
                expires: DateTime.Now.AddMinutes(60),
                signingCredentials: signingCredentials,
                claims: claims
                );

            return Task.FromResult(token);
        }
        #endregion
    }
}
