using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Users.Infrastructure.Data;
using Users.Infrastructure.Models;
using Users.Infrastructure.Models.Dtos;
using Users.Infrastructure.Repository;
using Users.Application.Services.IService;
using System;
using System.Linq;

namespace Users.Application.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IReviewRepository _reviewRepository;
        public ReviewService(IReviewRepository reviewRepository)
        {
            _reviewRepository = reviewRepository;
        }

        public async Task<IEnumerable<Review>> GetAllAsync()
        {
            return await _reviewRepository.GetAllAsync();
        }

        public async Task<Review?> GetByIdAsync(int id)
        {
            return await _reviewRepository.GetByIdAsync(id);
        }

        public async Task<Review> CreateAsync(ReviewDTO dto)
        {
            var review = new Review
            {
                LawyerId = dto.LawyerId,
                UserId = dto.UserId,
                Rating = dto.Rating,
                Comment = dto.Comment,
                CreatedAt = DateTime.UtcNow
            };
            await _reviewRepository.AddAsync(review);
            return review;
        }

        public async Task<Review?> UpdateAsync(int id, ReviewDTO dto)
        {
            var review = await _reviewRepository.GetByIdAsync(id);
            if (review == null) return null;
            review.LawyerId = dto.LawyerId;
            review.UserId = dto.UserId;
            review.Rating = dto.Rating;
            review.Comment = dto.Comment;
            await _reviewRepository.UpdateAsync(review);
            return review;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var review = await _reviewRepository.GetByIdAsync(id);
            if (review == null) return false;
            await _reviewRepository.DeleteAsync(id);
            return true;
        }

        public async Task<IEnumerable<Review>> GetReviewsByLawyerIdAsync(int lawyerId)
        {
            var allReviews = await _reviewRepository.GetAllAsync();
            return allReviews.Where(r => r.LawyerId == lawyerId);
        }

        public async Task<double> GetAverageRatingByLawyerIdAsync(int lawyerId)
        {
            var reviews = await GetReviewsByLawyerIdAsync(lawyerId);
            if (!reviews.Any()) return 0;
            return reviews.Average(r => (double)r.Rating);
        }
    }
} 