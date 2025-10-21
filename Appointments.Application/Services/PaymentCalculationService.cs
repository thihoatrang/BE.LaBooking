using Appointments.Infrastructure.Models.Dtos;


namespace Appointments.Application.Services
{
    public interface IPaymentCalculationService
    {
        Task<long> CalculatePaymentAmountAsync(int lawyerId, int durationHours = 1);
        Task<long> CalculatePaymentAmountAsync(CreateAppointmentDTO appointmentDto, int durationHours = 1);
    }

    public class PaymentCalculationService : IPaymentCalculationService
    {
        private readonly LawyerProfileApiClient _lawyerApiClient;

        public PaymentCalculationService(LawyerProfileApiClient lawyerApiClient)
        {
            _lawyerApiClient = lawyerApiClient;
        }

        public async Task<long> CalculatePaymentAmountAsync(int lawyerId, int durationHours = 1)
        {
            try
            {
                var lawyerProfile = await _lawyerApiClient.GetLawyerProfileByIdAsync(lawyerId);
                if (lawyerProfile == null)
                {
                    throw new ArgumentException($"Lawyer with ID {lawyerId} not found");
                }

                // Calculate amount based on PricePerHour and duration
                var amount = (long)(lawyerProfile.PricePerHour * durationHours);
                return amount;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to calculate payment amount: {ex.Message}", ex);
            }
        }

        public async Task<long> CalculatePaymentAmountAsync(CreateAppointmentDTO appointmentDto, int durationHours = 1)
        {
            return await CalculatePaymentAmountAsync(appointmentDto.LawyerId, durationHours);
        }
    }
}
