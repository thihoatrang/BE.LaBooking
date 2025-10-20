using Appointments.Infrastructure.Models.Saga;
using Microsoft.EntityFrameworkCore;

namespace Appointments.Infrastructure.Data
{
    public class SagaDbContext : DbContext
    {
        public SagaDbContext(DbContextOptions<SagaDbContext> options) : base(options)
        {
        }

        public DbSet<SagaState> SagaStates { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<SagaState>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasMaxLength(50);
                entity.Property(e => e.SagaType).HasMaxLength(50).IsRequired();
                entity.Property(e => e.EntityId).HasMaxLength(50).IsRequired();
                entity.Property(e => e.State).HasMaxLength(50).IsRequired();
                entity.Property(e => e.Data).HasColumnType("nvarchar(max)");
                entity.Property(e => e.ErrorMessage).HasMaxLength(1000);
                
                entity.HasIndex(e => new { e.SagaType, e.EntityId });
                entity.HasIndex(e => e.CreatedAt);
            });
        }
    }
}
