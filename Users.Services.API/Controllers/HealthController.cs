using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using Users.Infrastructure.Data;

namespace Users.Application.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HealthController : ControllerBase
    {
        private readonly UserDbContext _context;

        public HealthController(UserDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [SwaggerOperation(
        Summary = "Kiểm tra kết nối DB User")]
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
                    Service = "Users Service",
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
                        Service = "Users Service",
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
                    Service = "Users Service",
                    Error = ex.Message
                });
            }
        }
    }
}
