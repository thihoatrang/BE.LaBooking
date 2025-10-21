using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Lawyers.Infrastructure.Models;
using Lawyers.Infrastructure.Data;

namespace Lawyers.Infrastructure.Repository
{
    public class ServiceRepository : IServiceRepository
    {
        private readonly LawyerDbContext _context;
        
        public ServiceRepository(LawyerDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Service>> GetAllAsync() => 
            await _context.Services.Include(s => s.PracticeArea).ToListAsync();

        public async Task<Service?> GetByIdAsync(int id) => 
            await _context.Services.Include(s => s.PracticeArea).FirstOrDefaultAsync(s => s.Id == id);

        public async Task<IEnumerable<Service>> GetByPracticeAreaIdAsync(int practiceAreaId) => 
            await _context.Services
                .Include(s => s.PracticeArea)
                .Where(s => s.PracticeAreaId == practiceAreaId)
                .ToListAsync();

        public async Task<Service?> GetByCodeAsync(string code) => 
            await _context.Services.Include(s => s.PracticeArea).FirstOrDefaultAsync(s => s.Code == code);

        public async Task AddAsync(Service service)
        {
            _context.Services.Add(service);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Service service)
        {
            _context.Services.Update(service);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var service = await _context.Services.FindAsync(id);
            if (service != null)
            {
                _context.Services.Remove(service);
                await _context.SaveChangesAsync();
            }
        }
    }
}
