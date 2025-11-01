using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Users.Application.Services.IService;
using Users.Infrastructure.Models.Dtos;

namespace Users.Services.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet("statistics")]
        [SwaggerOperation(
        Summary = "Lấy thống kê tổng quan của hệ thống - Payment, Appointment, Review")]
        public async Task<ActionResult<DashboardStatisticsDto>> GetDashboardStatistics(
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            try
            {
                var statistics = await _dashboardService.GetDashboardStatisticsAsync(fromDate, toDate);
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Error = "Failed to get dashboard statistics",
                    Details = ex.Message
                });
            }
        }
    }
}

