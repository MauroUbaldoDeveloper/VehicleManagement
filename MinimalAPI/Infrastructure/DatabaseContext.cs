using Microsoft.EntityFrameworkCore;
using MinimalAPI.Domain.Entities;

namespace MinimalAPI.Infrastructure
{
    public class DatabaseContext : DbContext
    {
        public DbSet<User> users { get; set; } = default!;
        public DbSet<Vehicle> vehicles { get; set; } = default!;
        private readonly IConfiguration _appSettingsConfig;


        public DatabaseContext(IConfiguration config)
        {
            _appSettingsConfig = config;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasData(
                 new User
                 {
                     Id = 1,
                     Email = "administrator@test.com",
                     Password = "123456",
                     Profile = "ADM"
                 }
                );
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var stringConection = _appSettingsConfig.GetConnectionString("sql")?.ToString();
                if (!string.IsNullOrEmpty(stringConection))
                    optionsBuilder.UseSqlServer(stringConection);
            }
        }
    }
}