using Appointments.Infrastructure.Models;
using Appointments.Infrastructure.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace Appointments.Infrastructure.Data
{
    public class AppointmentDbContext : DbContext
    {
        public AppointmentDbContext(DbContextOptions<AppointmentDbContext> options) : base(options)
        {
        }

        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Payment> Payments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Appointment>()
                .Property(a => a.Services)
                .HasConversion(
                    v => v == null ? "" : string.Join(',', v),
                    v => string.IsNullOrEmpty(v) ? new List<string>() : v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
                );

            modelBuilder.Entity<Appointment>()
                .Property(a => a.Status)
                .HasDefaultValue(AppointmentStatus.Pending);

            modelBuilder.Entity<Appointment>()
                .Property(a => a.IsDel)
                .HasDefaultValue(false);

            modelBuilder.Entity<Appointment>()
                .Property(a => a.CreateAt)
                .HasDefaultValueSql("GETDATE()");
        
        // Seed data
        //    modelBuilder.Entity<Appointment>().HasData(
        //        new Appointment
        //        {
        //            Id = 1,
        //            UserId = 7,
        //            LawyerId = 1,
        //            ScheduledAt = DateTime.Now.AddDays(1),
        //            Slot = "09:00-10:00",
        //            CreateAt = DateTime.Now,
        //            Status = AppointmentStatus.Pending,
        //            IsDel = false,
        //            Note = "First consultation",
        //            Spec = "Family Law",
        //            Services = new List<string> { "Legal Consultation", "Document Review" }
        //        },
        //        new Appointment
        //        {
        //            Id = 2,
        //            UserId = 7,
        //            LawyerId = 1,
        //            ScheduledAt = DateTime.Now.AddDays(2),
        //            Slot = "10:00-11:00",
        //            CreateAt = DateTime.Now,
        //            Status = AppointmentStatus.Confirmed,
        //            IsDel = false,
        //            Note = "Divorce case discussion",
        //            Spec = "Family Law",
        //            Services = new List<string> { "Legal Consultation" }
        //        },
        //        new Appointment
        //        {
        //            Id = 3,
        //            UserId = 7,
        //            LawyerId = 2,
        //            ScheduledAt = DateTime.Now.AddDays(1),
        //            Slot = "14:00-15:00",
        //            CreateAt = DateTime.Now,
        //            Status = AppointmentStatus.Completed,
        //            IsDel = false,
        //            Note = "Contract review completed",
        //            Spec = "Corporate Law",
        //            Services = new List<string> { "Contract Review", "Legal Consultation" }
        //        },
        //        new Appointment
        //        {
        //            Id = 4,
        //            UserId = 7,
        //            LawyerId = 2,
        //            ScheduledAt = DateTime.Now.AddDays(3),
        //            Slot = "11:00-12:00",
        //            CreateAt = DateTime.Now,
        //            Status = AppointmentStatus.Cancelled,
        //            IsDel = false,
        //            Note = "Client requested cancellation",
        //            Spec = "Corporate Law",
        //            Services = new List<string> { "Legal Consultation" }
        //        },
        //        new Appointment
        //        {
        //            Id = 5,
        //            UserId = 7,
        //            LawyerId = 3,
        //            ScheduledAt = DateTime.Now.AddDays(4),
        //            Slot = "15:00-16:00",
        //            CreateAt = DateTime.Now,
        //            Status = AppointmentStatus.Pending,
        //            IsDel = false,
        //            Note = "Criminal case consultation",
        //            Spec = "Criminal Law",
        //            Services = new List<string> { "Legal Consultation", "Case Analysis" }
        //        },
        //        new Appointment
        //        {
        //            Id = 6,
        //            UserId = 6,
        //            LawyerId = 3,
        //            ScheduledAt = DateTime.Now.AddDays(2),
        //            Slot = "16:00-17:00",
        //            CreateAt = DateTime.Now,
        //            Status = AppointmentStatus.Confirmed,
        //            IsDel = false,
        //            Note = "Follow-up meeting",
        //            Spec = "Criminal Law",
        //            Services = new List<string> { "Legal Consultation" }
        //        },
        //        new Appointment
        //        {
        //            Id = 7,
        //            UserId = 7,
        //            LawyerId = 4,
        //            ScheduledAt = DateTime.Now.AddDays(5),
        //            Slot = "09:00-10:00",
        //            CreateAt = DateTime.Now,
        //            Status = AppointmentStatus.Pending,
        //            IsDel = false,
        //            Note = "Property dispute consultation",
        //            Spec = "Property Law",
        //            Services = new List<string> { "Legal Consultation", "Document Review" }
        //        },
        //        new Appointment
        //        {
        //            Id = 8,
        //            UserId = 7,
        //            LawyerId = 4,
        //            ScheduledAt = DateTime.Now.AddDays(3),
        //            Slot = "13:00-14:00",
        //            CreateAt = DateTime.Now,
        //            Status = AppointmentStatus.Completed,
        //            IsDel = false,
        //            Note = "Property transfer completed",
        //            Spec = "Property Law",
        //            Services = new List<string> { "Document Preparation", "Legal Consultation" }
        //        }

        //    );
    }
    }
} 