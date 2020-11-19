using JustLearnIT.AzureServices;
using JustLearnIT.Models;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace JustLearnIT.Security
{
    public enum EmailType
    {
        Email_Verification = 64,
        Login_Verification = 8,
        Password_Restart = 16
    }

    public static class AuthService
    {
        #region JWT
        public static Task<JwtSecurityToken> AssignToken(UserModel user)
        {
            SymmetricSecurityKey symmetricKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(AzureServices.KeyVaultService.GetSecretByName("JWT--Key")));
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

        #region SMTP ACTIONS
        private static SmtpClient SMTP { get; set; }

        public static void CreateSMTPClient(string password)
        {
            SMTP = new SmtpClient
            {
                Port = 587,
                Host = "smtp.gmail.com",
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential("justlearnit.bot@gmail.com", password),
                DeliveryMethod = SmtpDeliveryMethod.Network
            };
        }

        public static async Task<string> SendEmail(string sendTo, string userName, EmailType type)
        {
            var generated = CreateRadnomString((int)type);

            MailMessage message = new MailMessage();
            message.From = new MailAddress("justlearnit.bot@gmail.com");
            message.To.Add(new MailAddress(sendTo));
            message.Subject = Enum.GetName(typeof(EmailType), type);
            message.IsBodyHtml = true;
            message.Body = await GetMessage(type, userName, generated);
            SMTP.Send(message);

            return generated;
        }

        private static async Task<string> GetMessage(EmailType type, string username, string generatedString)
        {
            string content = await BlobStorageService.GetTextFromFileByName(Enum.GetName(typeof(EmailType), type), "email");

            switch (type)
            {
                case EmailType.Email_Verification:
                    content = content.Replace("VER_URL", generatedString);
                    break;
                case EmailType.Login_Verification:
                    content = content.Replace("LOGIN_CODE", generatedString); // waiting for implementation
                    break;
                case EmailType.Password_Restart:
                    content = content.Replace("TEMP_PASS", generatedString); // waiting for implementation
                    break;
            }

            content = content.Replace("TITLE", Enum.GetName(typeof(EmailType), type));
            content = content.Replace("USER_NAME", username);

            return content;
        }

        private static string CreateRadnomString(int length)
        {
            using var rng = new RNGCryptoServiceProvider();
            var bytes = new byte[length];
            rng.GetBytes(bytes);
            return Regex.Replace(Convert.ToBase64String(bytes), "[/]", "x");
        }
        #endregion
    }
}
