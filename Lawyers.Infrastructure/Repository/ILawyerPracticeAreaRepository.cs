using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Lawyers.Infrastructure.Models;
using Lawyers.Infrastructure.Data;

namespace Lawyers.Infrastructure.Repository
{
    public interface ILawyerPracticeAreaRepository
    {
        Task<IEnumerable<LawyerPracticeArea>> GetAllAsync();
        Task<IEnumerable<LawyerPracticeArea>> GetByLawyerIdAsync(int lawyerId);
        Task<IEnumerable<LawyerPracticeArea>> GetByPracticeAreaIdAsync(int practiceAreaId);
        Task AddAsync(LawyerPracticeArea lawyerPracticeArea);
        Task DeleteAsync(int lawyerId, int practiceAreaId);
        Task DeleteByLawyerIdAsync(int lawyerId);
    }
}
