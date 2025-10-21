using System.Collections.Generic;
using System.Threading.Tasks;
using Lawyers.Infrastructure.Models;

namespace Lawyers.Infrastructure.Repository
{
    public interface ILawyerProfileRepository
    {
        Task<IEnumerable<LawyerProfile>> GetAllAsync();
        Task<LawyerProfile?> GetByIdAsync(int id);
        Task<LawyerProfile?> GetByIdWithPracticeAreasAsync(int id);
        Task<LawyerProfile?> GetByUserIdAsync(int userId);
        Task AddAsync(LawyerProfile profile);
        Task UpdateAsync(LawyerProfile profile);
        Task DeleteAsync(int id);
    }
} 