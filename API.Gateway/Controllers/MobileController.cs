using API.Gateway.Services;
using Microsoft.AspNetCore.Mvc;

namespace API.Gateway.Controllers
{
    [Route("api/mobile")]
    [ApiController]
    public class MobileController : ControllerBase
    {
        private readonly CrossServiceSagaService _sagaService;
        private readonly ILogger<MobileController> _logger;

        public MobileController(CrossServiceSagaService sagaService, ILogger<MobileController> logger)
        {
            _sagaService = sagaService;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<IActionResult> MobileLogin([FromBody] object loginData)
        {
            try
            {
                // Forward to Users service
                using var httpClient = new HttpClient();
                var response = await httpClient.PostAsJsonAsync("https://localhost:7000/api/auth/login", loginData);
                var result = await response.Content.ReadAsStringAsync();
                
                if (response.IsSuccessStatusCode)
                {
                    return Ok(new { 
                        Success = true, 
                        Data = result,
                        Message = "Mobile login successful"
                    });
                }
                
                return BadRequest(new { 
                    Success = false, 
                    Message = "Login failed",
                    Details = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Mobile login failed");
                return StatusCode(500, new { 
                    Success = false, 
                    Message = "Internal server error",
                    Details = ex.Message
                });
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> MobileRegister([FromBody] object registerData)
        {
            try
            {
                // Use cross-service saga for complete registration
                var result = await _sagaService.StartCompleteUserRegistrationSagaAsync(registerData);
                
                return Ok(new { 
                    Success = true, 
                    Data = result,
                    Message = "Mobile registration successful with saga pattern"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Mobile registration failed");
                return StatusCode(500, new { 
                    Success = false, 
                    Message = "Registration failed",
                    Details = ex.Message
                });
            }
        }

        [HttpPost("appointment")]
        public async Task<IActionResult> CreateAppointment([FromBody] object appointmentData)
        {
            try
            {
                // Use cross-service saga for appointment with validation
                var result = await _sagaService.StartAppointmentWithUserLawyerSagaAsync(appointmentData);
                
                return Ok(new { 
                    Success = true, 
                    Data = result,
                    Message = "Appointment created successfully with saga pattern"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Mobile appointment creation failed");
                return StatusCode(500, new { 
                    Success = false, 
                    Message = "Appointment creation failed",
                    Details = ex.Message
                });
            }
        }

        [HttpGet("lawyers")]
        public async Task<IActionResult> GetLawyers()
        {
            try
            {
                using var httpClient = new HttpClient();
                var response = await httpClient.GetAsync("https://localhost:7110/api/lawyers/GetAllLawyerProfile");
                var result = await response.Content.ReadAsStringAsync();
                
                if (response.IsSuccessStatusCode)
                {
                    return Ok(new { 
                        Success = true, 
                        Data = result,
                        Message = "Lawyers retrieved successfully"
                    });
                }
                
                return BadRequest(new { 
                    Success = false, 
                    Message = "Failed to retrieve lawyers",
                    Details = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get lawyers for mobile");
                return StatusCode(500, new { 
                    Success = false, 
                    Message = "Internal server error",
                    Details = ex.Message
                });
            }
        }

        [HttpGet("user/{userId}/appointments")]
        public async Task<IActionResult> GetUserAppointments(int userId)
        {
            try
            {
                using var httpClient = new HttpClient();
                var response = await httpClient.GetAsync($"https://localhost:7001/api/appointments/user/{userId}");
                var result = await response.Content.ReadAsStringAsync();
                
                if (response.IsSuccessStatusCode)
                {
                    return Ok(new { 
                        Success = true, 
                        Data = result,
                        Message = "User appointments retrieved successfully"
                    });
                }
                
                return BadRequest(new { 
                    Success = false, 
                    Message = "Failed to retrieve appointments",
                    Details = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get user appointments for mobile");
                return StatusCode(500, new { 
                    Success = false, 
                    Message = "Internal server error",
                    Details = ex.Message
                });
            }
        }

        [HttpGet("health")]
        public IActionResult HealthCheck()
        {
            return Ok(new { 
                Success = true, 
                Message = "Mobile API Gateway is healthy",
                Timestamp = DateTime.UtcNow,
                Services = new {
                    Users = "https://localhost:7000",
                    Lawyers = "https://localhost:7110", 
                    Appointments = "https://localhost:7001"
                }
            });
        }
    }
}
