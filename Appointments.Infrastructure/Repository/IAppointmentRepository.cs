using System.Collections.Generic;
using System.Threading.Tasks;
using Appointments.Infrastructure.Models;

namespace Appointments.Infrastructure.Repository
{
    public interface IAppointmentRepository
    {
        Task<IEnumerable<Appointment>> GetAllAsync();
        Task<Appointment?> GetByIdAsync(int id);
        Task AddAsync(Appointment appointment);
        Task UpdateAsync(Appointment appointment);
        Task DeleteAsync(int id);
    }
} 