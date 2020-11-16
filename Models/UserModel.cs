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
        [StringLength(20, MinimumLength = 4)]
        [RegularExpression("[a-zA-Z0-9]{4,20}")]
        public string Login { get; set; }

        [NotNull]
        [Required]
        [StringLength(256, MinimumLength = 6)]
        public string Password { get; set; }

        public DateTime AccountCreationTime { get; set; } = DateTime.Now;

        public Role Role { get; set; } = Role.USER;

        public bool Subscription { get; set; } = false;
    }
}
