using JustLearnIT.Models;
using Microsoft.EntityFrameworkCore;

namespace JustLearnIT.Data
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions options) : base(options)
        {
            
        }

        public DbSet<UserModel> Users { get; set; }
        public DbSet<CourseModel> Courses { get; set; }
        public DbSet<SaltModel> Salts { get; set; }
        public DbSet<VerificationCodeModel> VerificationCodes { get; set; }
        public DbSet<OneTimePassword> OneTimePasswords { get; set; }
    }
}
