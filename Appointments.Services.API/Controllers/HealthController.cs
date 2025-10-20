using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Appointments.Infrastructure.Data;

namespace Appointments.Services.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HealthController : ControllerBase
    {
        private readonly AppointmentDbContext _appointmentContext;
        private readonly SagaDbContext _sagaContext;

        public HealthController(AppointmentDbContext appointmentContext, SagaDbContext sagaContext)
        {
            _appointmentContext = appointmentContext;
            _sagaContext = sagaContext;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                // Check database connectivity
                var appointmentDbHealthy = await _appointmentContext.Database.CanConnectAsync();
                var sagaDbHealthy = await _sagaContext.Database.CanConnectAsync();

                var healthStatus = new
                {
                    Status = "Healthy",
                    Timestamp = DateTime.UtcNow,
                    Service = "Appointments Service",
                    Version = "1.0.0",
                    Database = new
                    {
                        AppointmentDb = appointmentDbHealthy ? "Connected" : "Disconnected",
                        SagaDb = sagaDbHealthy ? "Connected" : "Disconnected"
                    },
                    Uptime = Environment.TickCount64
                };

                if (!appointmentDbHealthy || !sagaDbHealthy)
                {
                    return StatusCode(503, new
                    {
                        Status = "Unhealthy",
                        Timestamp = DateTime.UtcNow,
                        Service = "Appointments Service",
                        Database = new
                        {
                            AppointmentDb = appointmentDbHealthy ? "Connected" : "Disconnected",
                            SagaDb = sagaDbHealthy ? "Connected" : "Disconnected"
                        }
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
                    Service = "Appointments Service",
                    Error = ex.Message
                });
            }
        }
    }
}
