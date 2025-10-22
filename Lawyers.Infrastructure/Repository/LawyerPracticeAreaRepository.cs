using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Lawyers.Infrastructure.Models;
using Lawyers.Infrastructure.Data;

namespace Lawyers.Infrastructure.Repository
{
    public class LawyerPracticeAreaRepository : ILawyerPracticeAreaRepository
    {
        private readonly LawyerDbContext _context;
        
        public LawyerPracticeAreaRepository(LawyerDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<LawyerPracticeArea>> GetAllAsync() => 
            await _context.LawyerPracticeAreas
                .Include(lpa => lpa.Lawyer)
                .Include(lpa => lpa.PracticeArea)
                .ToListAsync();

        public async Task<IEnumerable<LawyerPracticeArea>> GetByLawyerIdAsync(int lawyerId) => 
            await _context.LawyerPracticeAreas
                .Include(lpa => lpa.PracticeArea)
                .Where(lpa => lpa.LawyerId == lawyerId)
                .ToListAsync();

        public async Task<IEnumerable<LawyerPracticeArea>> GetByPracticeAreaIdAsync(int practiceAreaId) => 
            await _context.LawyerPracticeAreas
                .Include(lpa => lpa.Lawyer)
                .Where(lpa => lpa.PracticeAreaId == practiceAreaId)
                .ToListAsync();

        public async Task AddAsync(LawyerPracticeArea lawyerPracticeArea)
        {
            _context.LawyerPracticeAreas.Add(lawyerPracticeArea);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int lawyerId, int practiceAreaId)
        {
            var lawyerPracticeArea = await _context.LawyerPracticeAreas
                .FirstOrDefaultAsync(lpa => lpa.LawyerId == lawyerId && lpa.PracticeAreaId == practiceAreaId);
            
            if (lawyerPracticeArea != null)
            {
                _context.LawyerPracticeAreas.Remove(lawyerPracticeArea);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteByLawyerIdAsync(int lawyerId)
        {
            var lawyerPracticeAreas = await _context.LawyerPracticeAreas
                .Where(lpa => lpa.LawyerId == lawyerId)
                .ToListAsync();
            
            if (lawyerPracticeAreas.Any())
            {
                _context.LawyerPracticeAreas.RemoveRange(lawyerPracticeAreas);
                await _context.SaveChangesAsync();
            }
        }
    }
}
