using Appointments.Infrastructure.Models;
using System.Collections.Generic;

namespace Appointments.Infrastructure.Repository
{
    public interface IPaymentRepository
    {
        Task<Payment?> GetByOrderIdAsync(string orderId, CancellationToken ct = default);
        Task<Payment?> GetByAppointmentIdAsync(int appointmentId, CancellationToken ct = default);
        Task<IEnumerable<Payment>> GetAllByAppointmentIdAsync(int appointmentId, CancellationToken ct = default);
        Task<(IEnumerable<Payment> Payments, int TotalCount)> GetAllAsync(
            int page = 1, 
            int pageSize = 10, 
            string? status = null, 
            string? vendor = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            CancellationToken ct = default);
        Task<(IEnumerable<Payment> Payments, int TotalCount)> GetByUserIdAsync(
            int userId,
            int page = 1,
            int pageSize = 10,
            string? status = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            CancellationToken ct = default);
        Task UpsertAsync(Payment payment, CancellationToken ct = default);
    }
}


