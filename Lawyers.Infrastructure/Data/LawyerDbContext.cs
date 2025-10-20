using Lawyers.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Lawyers.Infrastructure.Data
{
    public class LawyerDbContext : DbContext
    {
        public LawyerDbContext(DbContextOptions<LawyerDbContext> options) : base(options) { }

        public DbSet<LawyerProfile> LawyerProfiles { get; set; }
        public DbSet<LawyerDiploma> Diplomas { get; set; }
        public DbSet<WorkSlot> WorkSlots { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var specConverter = new ValueConverter<List<string>, string>(
                v => string.Join(',', v),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
            );

            var specComparer = new ValueComparer<List<string>>(
                (c1, c2) => c1.SequenceEqual(c2),
                c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                c => c.ToList()
            );

            modelBuilder.Entity<LawyerProfile>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Bio).HasMaxLength(1000);
                entity.Property(e => e.LicenseNum).IsRequired();
                entity.Property(e => e.Description).HasMaxLength(200);

                entity.Property(e => e.Spec)
                      .HasConversion(specConverter)
                      .Metadata.SetValueComparer(specComparer);
            });

            modelBuilder.Entity<LawyerDiploma>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("Diplomas", tb => tb.HasTrigger("TRG_Diplomas_Update")); // Thêm HasTrigger, thay tên trigger nếu cần

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.QualificationType)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.IssuedBy)
                    .HasMaxLength(255);

                entity.HasOne(d => d.LawyerProfile)
                    .WithMany()
                    .HasForeignKey(d => d.LawyerId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<WorkSlot>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.DayOfWeek).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Slot).IsRequired().HasMaxLength(255);
                entity.Property(e => e.IsActive).IsRequired();

                entity.HasOne(ws => ws.Lawyer)
                    .WithMany()
                    .HasForeignKey(ws => ws.LawyerId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Seed data
            modelBuilder.Entity<LawyerProfile>().HasData(
                new LawyerProfile
                {
                    Id = 1,
                    UserId = 1,
                    Bio = "Luật sư chuyên dân sự, hơn 10 năm kinh nghiệm.",
                    Spec = new List<string> { "Dân sự", "Hợp đồng" },
                    LicenseNum = "LS1234",
                    ExpYears = 10,
                    Description = "Hà Nội",
                    Rating = 4.9,
                    PricePerHour = 500000,
                    Img = "lawyer1.jpg",
                    DayOfWeek = "Mon,Tue,Wed",
                    WorkTime = "08:00-12:00"
                },
                new LawyerProfile
                {
                    Id = 2,
                    UserId = 2,
                    Bio = "Luật sư hình sự, tư vấn luật hơn 8 năm.",
                    Spec = new List<string> { "Hình sự", "Tố tụng" },
                    LicenseNum = "LS5678",
                    ExpYears = 8,
                    Description = "TP. HCM",
                    Rating = 4.7,
                    PricePerHour = 600000,
                    Img = "lawyer2.jpg",
                    DayOfWeek = "Thu,Fri",
                    WorkTime = "13:00-17:00"
                },
                new LawyerProfile
                {
                    Id = 3,
                    UserId = 3,
                    Bio = "Luật sư chuyên về đất đai và bất động sản.",
                    Spec = new List<string> { "Đất đai", "Bất động sản" },
                    LicenseNum = "LS9001",
                    ExpYears = 12,
                    Description = "Đà Nẵng",
                    Rating = 4.8,
                    PricePerHour = 700000,
                    Img = "lawyer3.jpg",
                    DayOfWeek = "Mon,Wed,Fri",
                    WorkTime = "09:00-11:00"
                },
                new LawyerProfile
                {
                    Id = 4,
                    UserId = 4,
                    Bio = "Luật sư trẻ đầy nhiệt huyết, chuyên luật doanh nghiệp.",
                    Spec = new List<string> { "Doanh nghiệp", "Hợp đồng" },
                    LicenseNum = "LS7004",
                    ExpYears = 5,
                    Description = "Hải Phòng",
                    Rating = 4.6,
                    PricePerHour = 400000,
                    Img = "lawyer4.jpg",
                    DayOfWeek = "Tue,Thu",
                    WorkTime = "14:00-17:00"
                },
                new LawyerProfile
                {
                    Id = 5,
                    UserId = 5,
                    Bio = "Luật sư có chuyên môn sâu về hôn nhân gia đình.",
                    Spec = new List<string> { "Hôn nhân", "Ly hôn", "Nuôi con" },
                    LicenseNum = "LS3010",
                    ExpYears = 7,
                    Description = "Cần Thơ",
                    Rating = 4.85,
                    PricePerHour = 450000,
                    Img = "lawyer5.jpg",
                    DayOfWeek = "Sat,Sun",
                    WorkTime = "08:00-12:00"
                }
            );

            modelBuilder.Entity<WorkSlot>().HasData(
                new WorkSlot { Id = 1, LawyerId = 1, DayOfWeek = "Thứ Hai", Slot = "09:00 - 10:00", IsActive = true },
                new WorkSlot { Id = 2, LawyerId = 1, DayOfWeek = "Thứ Ba", Slot = "14:00 - 15:00", IsActive = true },
                new WorkSlot { Id = 3, LawyerId = 2, DayOfWeek = "Thứ Tư", Slot = "10:00 - 11:00", IsActive = true },
                new WorkSlot { Id = 4, LawyerId = 2, DayOfWeek = "Thứ Năm", Slot = "16:00 - 17:00", IsActive = false }
            );
        }
    }
}
