using Appointments.Infrastructure.Models;
using Appointments.Infrastructure.Models.Dtos;   

namespace Appointments.Application.Services.IService
{
    public interface IAppointmentService
    {
        Task<Appointment> CreateAppointmentAsync(CreateAppointmentDTO dto);
        Task<bool> UpdateConfirmedStatusAsync(int id);
        Task<bool> UpdateCancelledStatusAsync(int id);
        Task<bool> UpdateCompletedStatusAsync(int id);
        Task<bool> DeleteStatusAsync(int id);
        Task<Appointment?> UpdateAppointmentAsync(int id, UpdateAppointmentDTO dto);


    }
} 