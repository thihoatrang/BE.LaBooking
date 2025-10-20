using Appointments.Infrastructure.Models.Saga;
using Appointments.Infrastructure.Models.Dtos;

namespace Appointments.Application.Services.Saga
{
    public interface IAppointmentSagaService
    {
        Task<AppointmentSagaData> StartSagaAsync(CreateAppointmentDTO dto);
        Task<bool> CompleteSagaAsync(int appointmentId);
        Task<bool> CompensateSagaAsync(int appointmentId, string reason);
        Task<AppointmentSagaData?> GetSagaStateAsync(int appointmentId);
        Task<bool> UpdateSagaStateAsync(int appointmentId, AppointmentSagaState newState, string? errorMessage = null);
    }
}
