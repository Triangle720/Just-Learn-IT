using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace JustLearnIT.Models
{
    public enum Role
    {
        ADMIN,
        USER
    }

    public class UserModel
    {
        public string Id { get; set; }

        [NotNull]
        [Required]
        [RegularExpression("^[a-zA-Z0-9]{4,20}$", ErrorMessage = "4-20 chars, no special char.")]
        public string Login { get; set; }

        // Regex used only to get password from user. In database, it contain array of bytes
        [NotNull]
        [Required]
        [RegularExpression("^[ -!#-&(-~]{6,256}$", ErrorMessage = "6-256 chars, ASCII only")]
        public byte[] Password { get; set; }

        // about Email regex: sometimes people are using upper letters in emails... + email id may contain '.' (though it mean nothing (gmail))
        [NotNull]
        [Required]
        [RegularExpression(@"^[a-zA-Z0-9][a-zA-Z0-9.]*[a-zA-Z0-9]@[a-zA-Z0-9.]*\.[a-zA-Z]{1,3}$", ErrorMessage = "wrong e-mail format")]
        public string Email { get; set; }

        public DateTime AccountCreationTime { get; set; }

        public Role Role { get; set; }

        public bool Subscription { get; set; } = false;

        public bool IsVerified { get; set; } = false;

        public virtual SaltModel Salt { get; set; }
        public virtual VerificationCodeModel VerificationCode { get; set; }
        public virtual OneTimePassword OneTimePass { get; set; }
    }
}
