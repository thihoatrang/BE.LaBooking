using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Lawyers.Infrastructure.Models;
using Lawyers.Infrastructure.Data;

namespace Lawyers.Infrastructure.Repository
{
    public class LawyerProfileRepository : ILawyerProfileRepository
    {
        private readonly LawyerDbContext _context;
        public LawyerProfileRepository(LawyerDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<LawyerProfile>> GetAllAsync() => await _context.LawyerProfiles.ToListAsync();
        public async Task<LawyerProfile?> GetByIdAsync(int id) => await _context.LawyerProfiles.FindAsync(id);
        public async Task AddAsync(LawyerProfile profile)
        {
            _context.LawyerProfiles.Add(profile);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateAsync(LawyerProfile profile)
        {
            _context.LawyerProfiles.Update(profile);
            await _context.SaveChangesAsync();
        }
        public async Task DeleteAsync(int id)
        {
            var profile = await _context.LawyerProfiles.FindAsync(id);
            if (profile != null)
            {
                _context.LawyerProfiles.Remove(profile);
                await _context.SaveChangesAsync();
            }
        }
    }
} 