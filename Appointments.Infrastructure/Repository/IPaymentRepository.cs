using Appointments.Infrastructure.Models;

namespace Appointments.Infrastructure.Repository
{
    public interface IPaymentRepository
    {
        Task<Payment?> GetByOrderIdAsync(string orderId, CancellationToken ct = default);
        Task UpsertAsync(Payment payment, CancellationToken ct = default);
    }
}


