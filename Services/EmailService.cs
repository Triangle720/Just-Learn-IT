using System;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace JustLearnIT.Services
{
    public enum EmailType
    {
        Email_Verification = 64,
        Login_Verification = 8,
        Password_Restart = 16
    }

    public static class EmailService
    {
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
            string content = await BlobStorageService.GetTextFromFileByName(Enum.GetName(typeof(EmailType), type) + ".html", "email");
            content = content.Replace("GENERATED", generatedString);
            content = content.Replace("USER_NAME", username);
            return content;
        }

        private static string CreateRadnomString(int length)
        {
            using var rng = new RNGCryptoServiceProvider();
            var bytes = new byte[length];
            rng.GetBytes(bytes);
            return Regex.Replace(Convert.ToBase64String(bytes), "[/+]", "x");
        }
    }
}
