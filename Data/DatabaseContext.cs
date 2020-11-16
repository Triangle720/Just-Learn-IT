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

        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{

        //}
    }
}
