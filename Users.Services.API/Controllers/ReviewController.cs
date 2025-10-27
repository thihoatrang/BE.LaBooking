using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Collections.Generic;
using System.Threading.Tasks;
using Users.Application.Services.IService;
using Users.Infrastructure.Models;
using Users.Infrastructure.Models.Dtos;

namespace Users.Services.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewService _reviewService;
        public ReviewController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        [HttpGet]
        [SwaggerOperation(
        Summary = "Xem tất cả đánh giá")]
        public async Task<ActionResult<IEnumerable<Review>>> GetAll()
        {
            return Ok(await _reviewService.GetAllAsync());
        }

        [HttpGet("{id}")]
        [SwaggerOperation(
        Summary = "Xem đánh giá bằng id")]
        public async Task<ActionResult<Review>> GetById(int id)
        {
            var review = await _reviewService.GetByIdAsync(id);
            if (review == null) return NotFound();
            return Ok(review);
        }

        [HttpPost]
        [SwaggerOperation(
        Summary = "Tạo đánh giá")]
        public async Task<ActionResult<Review>> Create([FromBody] ReviewDTO dto)
        {
            var review = await _reviewService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = review.Id }, review);
        }

        [HttpPut("{id}")]
        [SwaggerOperation(
        Summary = "Sửa dịch vụ")]
        public async Task<ActionResult<Review>> Update(int id, [FromBody] ReviewDTO dto)
        {
            var review = await _reviewService.UpdateAsync(id, dto);
            if (review == null) return NotFound();
            return Ok(review);
        }

        [HttpDelete("{id}")]
        [SwaggerOperation(
        Summary = "Xóa đánh giá")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _reviewService.DeleteAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }

        [HttpGet("lawyer/{lawyerId}")]
        [SwaggerOperation(
        Summary = "Xem đánh giá của luật sư")]
        public async Task<ActionResult<IEnumerable<Review>>> GetReviewsByLawyerId(int lawyerId)
        {
            var reviews = await _reviewService.GetReviewsByLawyerIdAsync(lawyerId);
            return Ok(reviews);
        }

        [HttpGet("lawyer/{lawyerId}/average-rating")]
        [SwaggerOperation(
        Summary = "Xem tổng số trung bình sao của luật sư")]
        public async Task<ActionResult<double>> GetAverageRatingByLawyerId(int lawyerId)
        {
            var avg = await _reviewService.GetAverageRatingByLawyerIdAsync(lawyerId);
            return Ok(avg);
        }
    }
} 