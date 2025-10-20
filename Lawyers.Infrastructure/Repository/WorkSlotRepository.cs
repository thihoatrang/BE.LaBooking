using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Lawyers.Infrastructure.Models;
using Lawyers.Infrastructure.Data;

namespace Lawyers.Infrastructure.Repository
{
    public class WorkSlotRepository : IWorkSlotRepository
    {
        private readonly LawyerDbContext _context;
        public WorkSlotRepository(LawyerDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<WorkSlot>> GetAllAsync() => await _context.WorkSlots.ToListAsync();
        public async Task<WorkSlot?> GetByIdAsync(int id) => await _context.WorkSlots.FindAsync(id);
        public async Task AddAsync(WorkSlot workSlot)
        {
            _context.WorkSlots.Add(workSlot);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateAsync(WorkSlot workSlot)
        {
            _context.WorkSlots.Update(workSlot);
            await _context.SaveChangesAsync();
        }
        public async Task DeleteAsync(int id)
        {
            var ws = await _context.WorkSlots.FindAsync(id);
            if (ws != null)
            {
                _context.WorkSlots.Remove(ws);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<WorkSlot>> GetByLawyerIdAsync(int lawyerId)
        {
            return await _context.WorkSlots.Where(ws => ws.LawyerId == lawyerId).ToListAsync();
        }
    }
}