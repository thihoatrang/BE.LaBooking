using Appointments.Infrastructure.Models.Dtos;

namespace Appointments.Application.Services
{
    public interface ITransactionService
    {
        Task CreateTransactionRecordsAsync(string orderId, long amount);
        Task<bool> UpdatePaymentStatusAsync(string orderId, string status, string? transactionId = null, string? message = null);
    }
}



