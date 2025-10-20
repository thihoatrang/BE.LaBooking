using System.Collections.Generic;
using System.Threading.Tasks;
using Lawyers.Infrastructure.Models;

namespace Lawyers.Infrastructure.Repository
{
    public interface ILawyerDiplomaRepository
    {
        Task<IEnumerable<LawyerDiploma>> GetAllAsync();
        Task<LawyerDiploma?> GetByIdAsync(int id);
        Task AddAsync(LawyerDiploma diploma);
        Task UpdateAsync(LawyerDiploma diploma);
        Task DeleteAsync(int id);
    }
} 