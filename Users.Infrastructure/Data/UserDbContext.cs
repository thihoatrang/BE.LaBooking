using Microsoft.EntityFrameworkCore;
using Users.Infrastructure.Models;

namespace Users.Infrastructure.Data
{
    public class UserDbContext : DbContext
    {
        public UserDbContext(DbContextOptions<UserDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Form> Forms { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.FullName)
                      .IsRequired()
                      .HasMaxLength(200);

                entity.Property(e => e.Email)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(e => e.Password)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(e => e.PhoneNumber)
                      .HasMaxLength(20);

                entity.Property(e => e.Role)
                      .HasMaxLength(50);

                entity.Property(e => e.IsActive)
                      .IsRequired();
            });

            // Seed data
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    FullName = "Nguyễn Văn A",
                    Email = "a@example.com",
                    Password = "password",
                    PhoneNumber = "0912345678",
                    Role = "Lawyer",
                    IsActive = true
                },
                new User
                {
                    Id = 2,
                    FullName = "Nguyễn Văn B",
                    Email = "b@example.com",
                    Password = "password",
                    PhoneNumber = "12313131",
                    Role = "Lawyer",
                    IsActive = true
                },
                new User
                {
                    Id = 3,
                    FullName = "Nguyễn Văn C",
                    Email = "c@example.com",
                    Password = "password",
                    PhoneNumber = "0912235678",
                    Role = "Lawyer",
                    IsActive = true
                },
                new User
                {
                    Id = 4,
                    FullName = "Nguyễn Văn D",
                    Email = "d@example.com",
                    Password = "password",
                    PhoneNumber = "12313222",
                    Role = "Lawyer",
                    IsActive = true
                },
                new User
                {
                    Id = 5,
                    FullName = "Trần Thị An",
                    Email = "e@example.com",
                    Password = "password",
                    PhoneNumber = "0987654321",
                    Role = "Lawyer",
                    IsActive = true
                },
                new User
                {
                    Id = 6,
                    FullName = "Lê Văn Viet",
                    Email = "f@example.com",
                    Password = "password",
                    PhoneNumber = "0977123456",
                    Role = "Admin",
                    IsActive = true
                },
                new User
                {
                    Id = 7,
                    FullName = "Lê Nguyễn Khánh",
                    Email = "f@example.com",
                    Password = "password",
                    PhoneNumber = "0977123456",
                    Role = "Customer",
                    IsActive = true
                }
            );
        }
    }
}
