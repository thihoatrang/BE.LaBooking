using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Users.Infrastructure.Models;
using Users.Infrastructure.Data;

namespace Users.Infrastructure.Repository
{
    public class FormRepository : IFormRepository
    {
        private readonly UserDbContext _context;
        public FormRepository(UserDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<Form>> GetAllAsync() => await _context.Forms.ToListAsync();
        public async Task<Form?> GetByIdAsync(int id) => await _context.Forms.FindAsync(id);
        public async Task AddAsync(Form form)
        {
            _context.Forms.Add(form);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateAsync(Form form)
        {
            _context.Forms.Update(form);
            await _context.SaveChangesAsync();
        }
        public async Task DeleteAsync(int id)
        {
            var form = await _context.Forms.FindAsync(id);
            if (form != null)
            {
                _context.Forms.Remove(form);
                await _context.SaveChangesAsync();
            }
        }
    }
} 