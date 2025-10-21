using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Lawyers.Infrastructure.Models;
using Lawyers.Infrastructure.Data;

namespace Lawyers.Infrastructure.Repository
{
    public interface IPracticeAreaRepository
    {
        Task<IEnumerable<PracticeArea>> GetAllAsync();
        Task<PracticeArea?> GetByIdAsync(int id);
        Task<PracticeArea?> GetByCodeAsync(string code);
        Task AddAsync(PracticeArea practiceArea);
        Task UpdateAsync(PracticeArea practiceArea);
        Task DeleteAsync(int id);
    }
}
