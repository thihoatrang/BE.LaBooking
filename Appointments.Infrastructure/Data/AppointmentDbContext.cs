using Appointments.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace Appointments.Infrastructure.Data;

public partial class AppointmentDbContext : DbContext
{
    public AppointmentDbContext()
    {
    }

    public AppointmentDbContext(DbContextOptions<AppointmentDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Appointment> Appointments { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=(local);Uid=sa;Pwd=12345;Database=LA_Appointment;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Appointm__3214EC073CBEEEF9");

            entity.Property(e => e.Slot).HasMaxLength(50);
            entity.Property(e => e.Spec).HasMaxLength(100);
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Payments__3214EC07F14EDE72");

            entity.HasIndex(e => e.OrderId, "IX_Payments_OrderId").IsUnique();

            entity.Property(e => e.BankCode).HasMaxLength(50);
            entity.Property(e => e.BankTranNo).HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(GETDATE())"); // Use local time (Vietnam UTC+7)
            entity.Property(e => e.Message).HasMaxLength(500);
            entity.Property(e => e.OrderId).HasMaxLength(100);
            entity.Property(e => e.OrderInfo).HasMaxLength(500);
            entity.Property(e => e.PayDate).HasMaxLength(50);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("pending");
            entity.Property(e => e.TransactionId).HasMaxLength(100);
            entity.Property(e => e.Vendor).HasMaxLength(20);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
