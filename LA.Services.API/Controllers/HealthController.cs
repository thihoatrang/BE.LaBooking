using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Lawyers.Infrastructure.Data;

namespace LA.Services.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HealthController : ControllerBase
    {
        private readonly LawyerDbContext _context;

        public HealthController(LawyerDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                // Check database connectivity
                var dbHealthy = await _context.Database.CanConnectAsync();

                var healthStatus = new
                {
                    Status = "Healthy",
                    Timestamp = DateTime.UtcNow,
                    Service = "Lawyers Service",
                    Version = "1.0.0",
                    Database = dbHealthy ? "Connected" : "Disconnected",
                    Uptime = Environment.TickCount64
                };

                if (!dbHealthy)
                {
                    return StatusCode(503, new
                    {
                        Status = "Unhealthy",
                        Timestamp = DateTime.UtcNow,
                        Service = "Lawyers Service",
                        Database = "Disconnected"
                    });
                }

                return Ok(healthStatus);
            }
            catch (Exception ex)
            {
                return StatusCode(503, new
                {
                    Status = "Unhealthy",
                    Timestamp = DateTime.UtcNow,
                    Service = "Lawyers Service",
                    Error = ex.Message
                });
            }
        }
    }
}
