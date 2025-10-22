using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Lawyers.Infrastructure.Models;
using Lawyers.Infrastructure.Data;

namespace Lawyers.Infrastructure.Repository
{
    public interface IServiceRepository
    {
        Task<IEnumerable<Service>> GetAllAsync();
        Task<Service?> GetByIdAsync(int id);
        Task<IEnumerable<Service>> GetByPracticeAreaIdAsync(int practiceAreaId);
        Task<Service?> GetByCodeAsync(string code);
        Task AddAsync(Service service);
        Task UpdateAsync(Service service);
        Task DeleteAsync(int id);
    }
}
