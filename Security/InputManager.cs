using JustLearnIT.Data;
using JustLearnIT.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace JustLearnIT.Security
{
    internal static class InputManager
    {
        public static Regex Re { get; set; }
        // salt len, iterations
        public static int[] PBKDF2 { get; set; }

        public static async Task<byte[]> EncryptPassword(string password, string userId, DatabaseContext _context)
        {
            using var deriveBytes = new Rfc2898DeriveBytes(password, PBKDF2[0], PBKDF2[1]);

            var salt = new SaltModel
            {
                UserModelId = userId,
                Value = deriveBytes.Salt
            };
            var key = deriveBytes.GetBytes(PBKDF2[0]);

            await _context.AddAsync(salt);

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

        // protection against multi-accounting on one e-mail address
        public static string ParseEmail(string email)
        {
            email = email.ToLower();

            var emailId = email.Substring(0, email.IndexOf('@'));
            email = email.Remove(0, email.IndexOf('@'));

            if (emailId.Contains('.'))
            {
                emailId = Re.Replace(emailId, string.Empty);
            }

            return string.Concat(emailId, email);
        }
    }
}
