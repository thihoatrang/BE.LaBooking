using Appointments.Infrastructure.Data;
using Appointments.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace Appointments.Infrastructure.Repository
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly AppointmentDbContext _db;

        public PaymentRepository(AppointmentDbContext db)
        {
            _db = db;
        }

        public async Task<Payment?> GetByOrderIdAsync(string orderId, CancellationToken ct = default)
        {
            return await _db.Payments.AsNoTracking().FirstOrDefaultAsync(p => p.OrderId == orderId, ct);
        }

        public async Task UpsertAsync(Payment payment, CancellationToken ct = default)
        {
            var existing = await _db.Payments.FirstOrDefaultAsync(p => p.OrderId == payment.OrderId, ct);
            if (existing == null)
            {
                await _db.Payments.AddAsync(payment, ct);
            }
            else
            {
                existing.Status = payment.Status;
                existing.TransactionId = payment.TransactionId;
                existing.Message = payment.Message;
                existing.Amount = payment.Amount;
                existing.Vendor = payment.Vendor;
                existing.UpdatedAt = DateTime.UtcNow;
            }
            await _db.SaveChangesAsync(ct);
        }
    }
}


