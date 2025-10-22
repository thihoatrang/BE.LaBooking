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
        public DbSet<PracticeArea> PracticeAreas { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<LawyerPracticeArea> LawyerPracticeAreas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LawyerProfile>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Bio).HasMaxLength(1000);
                entity.Property(e => e.LicenseNum).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Description).HasMaxLength(200);
                entity.Property(e => e.DayOfWeek).IsRequired();
                entity.Property(e => e.WorkTime).IsRequired();
                entity.Property(e => e.IsActive).IsRequired();
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.UpdatedAt).IsRequired();
                
                // Unique constraint for UserId
                entity.HasIndex(e => e.UserId).IsUnique();
            });

            modelBuilder.Entity<PracticeArea>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Code).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(1000);
                
                // Unique constraint for Code
                entity.HasIndex(e => e.Code).IsUnique();
            });

            modelBuilder.Entity<Service>(entity =>
            {
                entity.ToTable("Service");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Code).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(1000);
                
                // Foreign key to PracticeArea
                entity.HasOne(s => s.PracticeArea)
                    .WithMany(pa => pa.Services)
                    .HasForeignKey(s => s.PracticeAreaId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                // Unique constraint for PracticeAreaId + Code
                entity.HasIndex(e => new { e.PracticeAreaId, e.Code }).IsUnique();
            });

            modelBuilder.Entity<LawyerPracticeArea>(entity =>
            {
                entity.HasKey(e => new { e.LawyerId, e.PracticeAreaId });
                
                // Foreign keys
                entity.HasOne(lpa => lpa.Lawyer)
                    .WithMany(lp => lp.LawyerPracticeAreas)
                    .HasForeignKey(lpa => lpa.LawyerId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                entity.HasOne(lpa => lpa.PracticeArea)
                    .WithMany(pa => pa.LawyerPracticeAreas)
                    .HasForeignKey(lpa => lpa.PracticeAreaId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<LawyerDiploma>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("Diplomas", tb => tb.HasTrigger("TRG_Diplomas_Update"));

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

            // Seed data for PracticeAreas
            modelBuilder.Entity<PracticeArea>().HasData(
                new PracticeArea { Id = 1, Code = "MARRIAGE", Name = "Hôn nhân & Gia đình", Description = "Ly hôn, nuôi con, tài sản chung, bạo lực gia đình..." },
                new PracticeArea { Id = 2, Code = "BUSINESS", Name = "Kinh doanh & Doanh nghiệp", Description = "Thành lập, điều lệ, cổ phần, M&A..." },
                new PracticeArea { Id = 3, Code = "INSURANCE", Name = "Bảo hiểm", Description = "Khiếu nại, bồi thường, hợp đồng bảo hiểm..." },
                new PracticeArea { Id = 4, Code = "CONTRACT", Name = "Hợp đồng", Description = "Soạn thảo, rà soát, đàm phán hợp đồng..." },
                new PracticeArea { Id = 5, Code = "LABOR", Name = "Lao động", Description = "HĐLĐ, kỷ luật lao động, tranh chấp lao động..." },
                new PracticeArea { Id = 6, Code = "CONSTRUCTION", Name = "Xây dựng", Description = "Giấy phép xây dựng, hợp đồng EPC, tranh chấp công trình..." }
            );

            // Seed data for Services
            modelBuilder.Entity<Service>().HasData(
                new Service { Id = 1, PracticeAreaId = 1, Code = "MARRIAGE_COUNSEL", Name = "Tư vấn pháp lý hôn nhân", Description = "Tư vấn tổng quát theo giờ" },
                new Service { Id = 2, PracticeAreaId = 1, Code = "MARRIAGE_DIVORCE", Name = "Tư vấn/soạn thảo hồ sơ ly hôn", Description = "Hướng dẫn thủ tục, soạn thảo hồ sơ" },
                new Service { Id = 3, PracticeAreaId = 2, Code = "BUSINESS_INHOUSE", Name = "Dịch vụ luật sư nội bộ", Description = "Tư vấn thường xuyên cho doanh nghiệp" },
                new Service { Id = 4, PracticeAreaId = 2, Code = "BUSINESS_DISPUTE", Name = "Tranh tụng & giải quyết tranh chấp", Description = "Đại diện làm việc, tố tụng" },
                new Service { Id = 5, PracticeAreaId = 3, Code = "INS_CLAIM_REVIEW", Name = "Đánh giá hồ sơ bồi thường", Description = "Rà soát điều khoản, chứng cứ" },
                new Service { Id = 6, PracticeAreaId = 4, Code = "CONTRACT_REVIEW", Name = "Đánh giá hợp đồng", Description = "Rà soát rủi ro, điều khoản bất lợi" },
                new Service { Id = 7, PracticeAreaId = 4, Code = "CONTRACT_DRAFT", Name = "Soạn thảo hợp đồng", Description = "Soạn hợp đồng theo yêu cầu khách hàng" },
                new Service { Id = 8, PracticeAreaId = 5, Code = "LABOR_CONSULT", Name = "Tư vấn pháp lý lao động", Description = "HĐLĐ, lương thưởng, kỷ luật" },
                new Service { Id = 9, PracticeAreaId = 6, Code = "CONST_LICENSE", Name = "Thủ tục giấy phép xây dựng", Description = "Hồ sơ, quy trình, thời hạn" }
            );

            // Seed data for LawyerPracticeAreas
            modelBuilder.Entity<LawyerPracticeArea>().HasData(
                new LawyerPracticeArea { LawyerId = 1, PracticeAreaId = 1 }, // Lawyer 1: Marriage
                new LawyerPracticeArea { LawyerId = 1, PracticeAreaId = 4 }, // Lawyer 1: Contract
                new LawyerPracticeArea { LawyerId = 2, PracticeAreaId = 2 }, // Lawyer 2: Business
                new LawyerPracticeArea { LawyerId = 2, PracticeAreaId = 4 }, // Lawyer 2: Contract
                new LawyerPracticeArea { LawyerId = 3, PracticeAreaId = 4 }, // Lawyer 3: Contract
                new LawyerPracticeArea { LawyerId = 3, PracticeAreaId = 6 }, // Lawyer 3: Construction
                new LawyerPracticeArea { LawyerId = 4, PracticeAreaId = 2 }, // Lawyer 4: Business
                new LawyerPracticeArea { LawyerId = 4, PracticeAreaId = 5 }, // Lawyer 4: Labor
                new LawyerPracticeArea { LawyerId = 5, PracticeAreaId = 1 }, // Lawyer 5: Marriage
                new LawyerPracticeArea { LawyerId = 5, PracticeAreaId = 3 }  // Lawyer 5: Insurance
            );

            // Updated seed data for LawyerProfiles (removed Spec)
            modelBuilder.Entity<LawyerProfile>().HasData(
                new LawyerProfile
                {
                    Id = 1,
                    UserId = 1,
                    Bio = "Luật sư chuyên dân sự, hơn 10 năm kinh nghiệm.",
                    LicenseNum = "LS1234",
                    ExpYears = 10,
                    Description = "Hà Nội",
                    Rating = 4.9,
                    PricePerHour = 500000,
                    Img = "lawyer1.jpg",
                    DayOfWeek = "Mon,Tue,Wed",
                    WorkTime = "08:00-12:00",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new LawyerProfile
                {
                    Id = 2,
                    UserId = 2,
                    Bio = "Luật sư hình sự, tư vấn luật hơn 8 năm.",
                    LicenseNum = "LS5678",
                    ExpYears = 8,
                    Description = "TP. HCM",
                    Rating = 4.7,
                    PricePerHour = 600000,
                    Img = "lawyer2.jpg",
                    DayOfWeek = "Thu,Fri",
                    WorkTime = "13:00-17:00",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new LawyerProfile
                {
                    Id = 3,
                    UserId = 3,
                    Bio = "Luật sư chuyên về đất đai và bất động sản.",
                    LicenseNum = "LS9001",
                    ExpYears = 12,
                    Description = "Đà Nẵng",
                    Rating = 4.8,
                    PricePerHour = 700000,
                    Img = "lawyer3.jpg",
                    DayOfWeek = "Mon,Wed,Fri",
                    WorkTime = "09:00-11:00",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new LawyerProfile
                {
                    Id = 4,
                    UserId = 4,
                    Bio = "Luật sư trẻ đầy nhiệt huyết, chuyên luật doanh nghiệp.",
                    LicenseNum = "LS7004",
                    ExpYears = 5,
                    Description = "Hải Phòng",
                    Rating = 4.6,
                    PricePerHour = 400000,
                    Img = "lawyer4.jpg",
                    DayOfWeek = "Tue,Thu",
                    WorkTime = "14:00-17:00",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new LawyerProfile
                {
                    Id = 5,
                    UserId = 5,
                    Bio = "Luật sư có chuyên môn sâu về hôn nhân gia đình.",
                    LicenseNum = "LS3010",
                    ExpYears = 7,
                    Description = "Cần Thơ",
                    Rating = 4.85,
                    PricePerHour = 450000,
                    Img = "lawyer5.jpg",
                    DayOfWeek = "Sat,Sun",
                    WorkTime = "08:00-12:00",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
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
