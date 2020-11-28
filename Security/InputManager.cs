using JustLearnIT.Data;
using JustLearnIT.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace JustLearnIT.Security
{
    internal static class InputManager
    {
        private static Regex Re { get; set; }
        // salt len, iterations
        private static int[] PBKDF2 { get; set; }

        public static void Create(string secret)
        {
            PBKDF2 = Array.ConvertAll(secret.Split(';'), s => int.Parse(s));
            Re = new Regex("[.]");
        }

        public static async Task<byte[]> EncryptPassword(string password, string userId, DatabaseContext _context)
        {
            using var deriveBytes = new Rfc2898DeriveBytes(password, PBKDF2[0], PBKDF2[1]);

            var salt = new SaltModel
            {
                UserModelId = userId,
                Value = deriveBytes.Salt
            };
            var key = deriveBytes.GetBytes(PBKDF2[0]);

            var userSalt = await _context.Salts.Where(s => s.UserModelId == userId).FirstOrDefaultAsync();
            if (userSalt != null)
            {
                userSalt.Value = salt.Value;
                _context.Entry(userSalt).State = EntityState.Modified;
            }
            else await _context.AddAsync(salt);

            return key;
        }

        public static async Task<bool> CheckPassword(string password, string userId, byte[] key, DatabaseContext _context)
        {
            var salt = await _context.Salts.Where(s => s.UserModelId == userId).FirstOrDefaultAsync();

            if (salt != null)
            {
                using var deriveBytes = new Rfc2898DeriveBytes(password, salt.Value, PBKDF2[1]);

                byte[] newKey = deriveBytes.GetBytes(PBKDF2[0]);

                if (!newKey.SequenceEqual(key))
                    return false;

                return true;
            }

            return false;
        }

        // protection against multi-accounting on one e-mail address (gmail)
        public static string ParseEmail(string email)
        {
            email = email.ToLower();

            if (email.Contains("@gmail"))
            {
                var emailId = email.Substring(0, email.IndexOf('@'));
                email = email.Remove(0, email.IndexOf('@'));

                if (emailId.Contains('.'))
                {
                    emailId = Re.Replace(emailId, string.Empty);
                }

                return string.Concat(emailId, email);
            }

            return email;
        }
    }
}
