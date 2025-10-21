using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Lawyers.Infrastructure.Models;
using Lawyers.Infrastructure.Data;

namespace Lawyers.Infrastructure.Repository
{
    public class PracticeAreaRepository : IPracticeAreaRepository
    {
        private readonly LawyerDbContext _context;
        
        public PracticeAreaRepository(LawyerDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PracticeArea>> GetAllAsync() => 
            await _context.PracticeAreas.ToListAsync();

        public async Task<PracticeArea?> GetByIdAsync(int id) => 
            await _context.PracticeAreas.FindAsync(id);

        public async Task<PracticeArea?> GetByCodeAsync(string code) => 
            await _context.PracticeAreas.FirstOrDefaultAsync(pa => pa.Code == code);

        public async Task AddAsync(PracticeArea practiceArea)
        {
            _context.PracticeAreas.Add(practiceArea);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(PracticeArea practiceArea)
        {
            _context.PracticeAreas.Update(practiceArea);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var practiceArea = await _context.PracticeAreas.FindAsync(id);
            if (practiceArea != null)
            {
                _context.PracticeAreas.Remove(practiceArea);
                await _context.SaveChangesAsync();
            }
        }
    }
}
