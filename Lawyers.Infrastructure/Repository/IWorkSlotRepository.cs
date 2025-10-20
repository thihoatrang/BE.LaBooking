using System.Collections.Generic;
using System.Threading.Tasks;
using Lawyers.Infrastructure.Models;

namespace Lawyers.Infrastructure.Repository
{
    public interface IWorkSlotRepository
    {
        Task<IEnumerable<WorkSlot>> GetAllAsync();
        Task<IEnumerable<WorkSlot>> GetByLawyerIdAsync(int lawyerId);
        Task<WorkSlot?> GetByIdAsync(int id);
        Task AddAsync(WorkSlot workSlot);
        Task UpdateAsync(WorkSlot workSlot);
        Task DeleteAsync(int id);
    }
}