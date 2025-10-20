using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Users.Infrastructure.Models;
using Users.Infrastructure.Data;

namespace Users.Infrastructure.Repository
{
    public class ReviewRepository : IReviewRepository
    {
        private readonly UserDbContext _context;
        public ReviewRepository(UserDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<Review>> GetAllAsync() => await _context.Reviews.ToListAsync();
        public async Task<Review?> GetByIdAsync(int id) => await _context.Reviews.FindAsync(id);
        public async Task AddAsync(Review review)
        {
            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateAsync(Review review)
        {
            _context.Reviews.Update(review);
            await _context.SaveChangesAsync();
        }
        public async Task DeleteAsync(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review != null)
            {
                _context.Reviews.Remove(review);
                await _context.SaveChangesAsync();
            }
        }
    }
} 