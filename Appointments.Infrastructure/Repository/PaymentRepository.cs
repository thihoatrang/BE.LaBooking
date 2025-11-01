using Appointments.Infrastructure.Data;
using Appointments.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

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

        public async Task<Payment?> GetByAppointmentIdAsync(int appointmentId, CancellationToken ct = default)
        {
            return await _db.Payments
                .AsNoTracking()
                .Where(p => p.AppointmentId == appointmentId)
                .OrderByDescending(p => p.CreatedAt)
                .FirstOrDefaultAsync(ct);
        }

        public async Task<IEnumerable<Payment>> GetAllByAppointmentIdAsync(int appointmentId, CancellationToken ct = default)
        {
            return await _db.Payments
                .AsNoTracking()
                .Where(p => p.AppointmentId == appointmentId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync(ct);
        }

        public async Task<(IEnumerable<Payment> Payments, int TotalCount)> GetAllAsync(
            int page = 1,
            int pageSize = 10,
            string? status = null,
            string? vendor = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            CancellationToken ct = default)
        {
            var query = _db.Payments.AsNoTracking();

            // Apply filters
            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(p => p.Status == status);
            }

            if (!string.IsNullOrEmpty(vendor))
            {
                query = query.Where(p => p.Vendor == vendor);
            }

            if (fromDate.HasValue)
            {
                query = query.Where(p => p.CreatedAt >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                // Add one day to include the entire end date
                var endDate = toDate.Value.AddDays(1);
                query = query.Where(p => p.CreatedAt < endDate);
            }

            // Get total count before pagination
            var totalCount = await query.CountAsync(ct);

            // Apply pagination and ordering
            var payments = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return (payments, totalCount);
        }

        public async Task<(IEnumerable<Payment> Payments, int TotalCount)> GetByUserIdAsync(
            int userId,
            int page = 1,
            int pageSize = 10,
            string? status = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            CancellationToken ct = default)
        {
            // Join Payments with Appointments to filter by UserId
            // Only include payments that have an AppointmentId and the appointment belongs to the user
            var query = from payment in _db.Payments.AsNoTracking()
                        where payment.AppointmentId.HasValue
                        join appointment in _db.Appointments.AsNoTracking()
                        on payment.AppointmentId.Value equals appointment.Id
                        where appointment.UserId == userId && !appointment.IsDel
                        select payment;

            // Apply filters
            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(p => p.Status == status);
            }

            if (fromDate.HasValue)
            {
                query = query.Where(p => p.CreatedAt >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                var endDate = toDate.Value.AddDays(1);
                query = query.Where(p => p.CreatedAt < endDate);
            }

            // Get total count before pagination
            var totalCount = await query.CountAsync(ct);

            // Apply pagination and ordering
            var payments = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return (payments, totalCount);
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
                existing.AppointmentId = payment.AppointmentId; // Update AppointmentId if provided
                existing.UpdatedAt = DateTime.Now; // Use local time (Vietnam UTC+7)
            }
            await _db.SaveChangesAsync(ct);
        }
    }
}


