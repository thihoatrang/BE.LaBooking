using Appointments.Infrastructure.Models.Dtos;
using Appointments.Infrastructure.Models.Enums;
using Appointments.Infrastructure.Repository;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Appointments.Application.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IConfiguration _configuration;
        private readonly ILogger<TransactionService> _logger;

        public TransactionService(
            IPaymentRepository paymentRepository,
            IAppointmentRepository appointmentRepository,
            IConfiguration configuration,
            ILogger<TransactionService> logger)
        {
            _paymentRepository = paymentRepository;
            _appointmentRepository = appointmentRepository;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task CreateTransactionRecordsAsync(string orderId, long amount)
        {
            try
            {
                _logger.LogInformation($"Creating transaction records for order {orderId} with amount {amount}");
                
                // In a real implementation, you would:
                // 1. Create transaction records in your database
                // 2. Update user balances
                // 3. Send notifications
                // 4. Update appointment status
                
                // For now, we'll just log the transaction
                _logger.LogInformation($"Transaction created: OrderId={orderId}, Amount={amount}");
                
                // You can extend this to:
                // - Create actual transaction records in database
                // - Update user/lawyer balances
                // - Send email notifications
                // - Update appointment status to confirmed
                
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to create transaction records for order {orderId}");
                throw;
            }
        }

        public async Task<bool> UpdatePaymentStatusAsync(string orderId, string status, string? transactionId = null, string? message = null)
        {
            try
            {
                var payment = await _paymentRepository.GetByOrderIdAsync(orderId);
                if (payment == null)
                {
                    _logger.LogWarning($"Payment not found for order {orderId}");
                    return false;
                }

                payment.Status = status;
                payment.TransactionId = transactionId;
                payment.Message = message;
                payment.UpdatedAt = DateTime.Now; // Use local time (Vietnam UTC+7)

                await _paymentRepository.UpsertAsync(payment);

                // If payment is successful, create transaction records and update appointment
                if (status == "success" && payment.Amount.HasValue)
                {
                    await CreateTransactionRecordsAsync(orderId, payment.Amount.Value);
                    
                    // Update appointment status if there's a linked appointment
                    if (payment.AppointmentId.HasValue && payment.AppointmentId > 0)
                    {
                        var appointment = await _appointmentRepository.GetByIdAsync(payment.AppointmentId.Value);
                        if (appointment != null)
                        {
                            var oldStatus = appointment.Status;
                            _logger.LogInformation($"Updating appointment {appointment.Id} from status {oldStatus} to Confirmed ({(int)AppointmentStatus.Confirmed}) after successful payment");
                            
                            appointment.Status = (int)AppointmentStatus.Confirmed;
                            await _appointmentRepository.UpdateAsync(appointment);
                            
                            // Verify the update
                            var updatedAppointment = await _appointmentRepository.GetByIdAsync(payment.AppointmentId.Value);
                            if (updatedAppointment != null && updatedAppointment.Status == (int)AppointmentStatus.Confirmed)
                            {
                                _logger.LogInformation($"Appointment {appointment.Id} confirmed successfully. Status changed from {oldStatus} to {updatedAppointment.Status}");
                            }
                            else
                            {
                                _logger.LogError($"Failed to verify appointment status update. Expected: {(int)AppointmentStatus.Confirmed}, Got: {updatedAppointment?.Status}");
                            }
                        }
                        else
                        {
                            _logger.LogWarning($"Appointment {payment.AppointmentId.Value} not found when trying to update after successful payment");
                        }
                    }
                    else
                    {
                        _logger.LogWarning($"Payment {orderId} succeeded but has no AppointmentId linked (AppointmentId: {payment.AppointmentId})");
                    }
                }
                // If payment failed, keep appointment in PaymentPending state for retry
                // Do NOT cancel automatically - user should be able to retry payment or cancel manually
                else if (status == "failed" && payment.AppointmentId.HasValue && payment.AppointmentId > 0)
                {
                    var appointment = await _appointmentRepository.GetByIdAsync(payment.AppointmentId.Value);
                    if (appointment != null)
                    {
                        // Keep appointment in PaymentPending state so user can retry
                        // Only log the failure, don't change appointment status
                        _logger.LogWarning($"Payment failed for appointment {appointment.Id}. Appointment remains in PaymentPending state for retry. OrderId: {orderId}, Message: {message}");
                        
                        // Optionally, you could add a retry count or timeout mechanism here
                        // For now, we keep the appointment active so user can retry payment
                    }
                    else
                    {
                        _logger.LogWarning($"Appointment {payment.AppointmentId.Value} not found when processing failed payment");
                    }
                }

                _logger.LogInformation($"Payment status updated: OrderId={orderId}, Status={status}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to update payment status for order {orderId}");
                return false;
            }
        }
    }
}




