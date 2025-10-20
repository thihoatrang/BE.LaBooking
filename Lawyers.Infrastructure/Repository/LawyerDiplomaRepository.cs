using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Lawyers.Infrastructure.Models;
using Lawyers.Infrastructure.Data;

namespace Lawyers.Infrastructure.Repository
{
    public class LawyerDiplomaRepository : ILawyerDiplomaRepository
    {
        private readonly LawyerDbContext _context;
        public LawyerDiplomaRepository(LawyerDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<LawyerDiploma>> GetAllAsync() => await _context.Diplomas.ToListAsync();
        public async Task<LawyerDiploma?> GetByIdAsync(int id) => await _context.Diplomas.FindAsync(id);
        public async Task AddAsync(LawyerDiploma diploma)
        {
            _context.Diplomas.Add(diploma);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateAsync(LawyerDiploma diploma)
        {
            _context.Diplomas.Update(diploma);
            await _context.SaveChangesAsync();
        }
        public async Task DeleteAsync(int id)
        {
            var diploma = await _context.Diplomas.FindAsync(id);
            if (diploma != null)
            {
                _context.Diplomas.Remove(diploma);
                await _context.SaveChangesAsync();
            }
        }
    }
} 