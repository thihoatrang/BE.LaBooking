using System.Collections.Generic;
using System.Threading.Tasks;
using Users.Infrastructure.Models;
using Users.Infrastructure.Models.Dtos;

namespace Users.Application.Services.IService
{
    public interface IReviewService
    {
        Task<IEnumerable<Review>> GetAllAsync();
        Task<Review?> GetByIdAsync(int id);
        Task<Review> CreateAsync(ReviewDTO dto);
        Task<Review?> UpdateAsync(int id, ReviewDTO dto);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<Review>> GetReviewsByLawyerIdAsync(int lawyerId);
        Task<double> GetAverageRatingByLawyerIdAsync(int lawyerId);
    }
}
