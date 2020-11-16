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
        [RegularExpression("[a-zA-Z0-9]{4,20}", ErrorMessage = "4-20 characters, a-z A-z 0-9")]
        public string Login { get; set; }

        [NotNull]
        [Required]
        [RegularExpression(".{6,256}", ErrorMessage = "6-256 characters")]
        public string Password { get; set; }

        public DateTime AccountCreationTime { get; set; } = DateTime.Now;

        public Role Role { get; set; } = Role.USER;

        public bool Subscription { get; set; } = false;
    }
}
