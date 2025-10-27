using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Users.Infrastructure.Models.Dtos;
using Users.Infrastructure.Models.Saga;
using Users.Application.Services.IService;
using Users.Application.Services.Saga;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Google.Apis.Auth;
using Swashbuckle.AspNetCore.Annotations;

namespace Users.Services.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IUserSagaService _sagaService;
        private readonly ILogger<AuthController> _logger;
        private readonly IConfiguration _configuration;

        public AuthController(
            IAuthService authService,
            IUserSagaService sagaService,
            ILogger<AuthController> logger, 
            IConfiguration configuration)
        {
            _authService = authService;
            _sagaService = sagaService;
            _logger = logger;
            _configuration = configuration;
        }


        [HttpPost("login")]
        [SwaggerOperation(
        Summary = "Đăng nhập")]
        public async Task<ActionResult<AuthResponseDTO>> Login([FromBody] LoginRequestDTO loginRequest)
        {
            var response = await _authService.LoginAsync(loginRequest);
            if (!response.IsSuccess)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpPost("register")]
        [SwaggerOperation(
        Summary = "Đăng ký")]
        public async Task<ActionResult<AuthResponseDTO>> Register([FromBody] RegisterRequestDTO registerRequest)
        {
            try
            {
                // Use Saga Pattern for user registration
                var sagaData = await _sagaService.StartUserRegistrationSagaAsync(registerRequest);

                if (sagaData?.UserId > 0)
                {
                    await _sagaService.SendNotificationEmailAsync(sagaData.UserId);
                }


                // Fallback to standard register response (no GenerateTokenAsync available)
                return Ok(new AuthResponseDTO
                {
                    IsSuccess = true,
                    Message = "OTP sent successfully using Saga Pattern"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new AuthResponseDTO
                {
                    IsSuccess = false,
                    Message = $"Registration failed: {ex.Message}"
                });
            }
        }

        [Authorize]
        [HttpPut("update/{userId}")]
        [SwaggerOperation(
        Summary = "Chỉnh thông tin người dùng bằng Id")]
        public async Task<ActionResult<ResponseDto<UserDTO>>> UpdateUser(int userId, [FromBody] UpdateUserDTO updateUserDto)
        {
            _logger.LogInformation("Authorization header: {Header}", Request.Headers["Authorization"].ToString());
            _logger.LogInformation("User ID from token: {UserId}", User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            
            var response = await _authService.UpdateUserAsync(userId, updateUserDto);
            if (!response.IsSuccess)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpGet("google-login")]
        [SwaggerOperation(
        Summary = "Login Google")]
        public IActionResult GoogleLogin()
        {
            try
            {
                var clientId = _configuration["GoogleAuthSettings:ClientId"];
                if (string.IsNullOrEmpty(clientId))
                {
                    return BadRequest(new { error = "Google Client ID is not configured" });
                }

                var redirectUri = "https://localhost:7071/api/auth/google-callback";
                var scope = "email profile";
                // Sửa lỗi: KHÔNG dùng access_type=offline với response_type=token
                var nonce = Guid.NewGuid().ToString("N");
                var googleAuthUrl = $"https://accounts.google.com/o/oauth2/v2/auth?" +
                    $"client_id={clientId}&" +
                    $"redirect_uri={redirectUri}&" +
                    $"response_type=id_token&" +
                    $"scope={scope}&" +
                    $"nonce={nonce}&" +
                    $"prompt=consent";


                _logger.LogInformation("Generated Google Auth URL with redirect URI: {RedirectUri}", redirectUri);
                return Ok(new {
                    url = googleAuthUrl,
                    redirectUri = redirectUri,
                    message = "Please make sure this redirect URI is added to your Google Cloud Console project"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating Google Auth URL");
                return BadRequest(new {
                    error = "Error generating Google Auth URL",
                    details = ex.Message
                });
            }
        }

        public class GoogleAccessTokenDTO
        {
            public string AccessToken { get; set; }
        }

        [HttpPost("google-callback")]
        public async Task<IActionResult> GoogleCallback([FromBody] GoogleAccessTokenDTO tokenDto)
        {
            try
            {
                var payload = await GoogleJsonWebSignature.ValidateAsync(tokenDto.AccessToken);
                if (payload == null)
                    return BadRequest(new { error = "Invalid Google access token" });

                var googleAuth = new GoogleAuthDTO
                {
                    Email = payload.Email,
                    Name = payload.Name,
                    Picture = payload.Picture
                };

                var response = await _authService.GoogleLoginAsync(googleAuth);
                if (!response.IsSuccess)
                    return BadRequest(response);

                // Trả JWT về để client JS redirect
                return Ok(new
                {
                    token = response.Token,
                    user = response.User,
                    message = "Google login successful"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }


        [HttpPost("verify-otp")]
        [SwaggerOperation(
        Summary = "Xác thực otp")]
        public async Task<ActionResult<AuthResponseDTO>> VerifyOtp([FromBody] VerifyOtpDTO verifyOtp)
        {
            _logger.LogInformation("Attempting to verify OTP for email: {Email}", verifyOtp.Email);
            var response = await _authService.VerifyOtpAsync(verifyOtp);
            
            if (!response.IsSuccess)
            {
                _logger.LogWarning("OTP verification failed: {Message}", response.Message);
                return BadRequest(response);
            }

            _logger.LogInformation("OTP verification successful for email: {Email}", verifyOtp.Email);
            return Ok(response);
        }

        [HttpDelete("delete/{email}")]
        [SwaggerOperation(
        Summary = "Xóa người dùng bằng email")]
        public async Task<ActionResult<ResponseDto<bool>>> DeleteUser(string email)
        {
            try
            {
                _logger.LogInformation("Attempting to delete user with email: {Email}", email);
                
                // Get user first to get ID for saga compensation
                var user = await _authService.GetUserByEmailAsync(email);
                if (user == null)
                {
                    return NotFound(new ResponseDto<bool>
                    {
                        IsSuccess = false,
                        Message = "User not found"
                    });
                }

                var response = await _authService.DeleteUserAsync(email);
                
                if (response.IsSuccess)
                {
                    // Compensate saga if user had one
                    await _sagaService.CompensateSagaAsync(user.Id, "User deleted by admin");
                    
                    _logger.LogInformation("Successfully deleted user with email: {Email}", email);
                    return Ok(new ResponseDto<bool>
                    {
                        IsSuccess = true,
                        Result = true,
                        Message = "User deleted successfully with saga compensation"
                    });
                }
                else
                {
                    _logger.LogWarning("Failed to delete user: {Message}", response.Message);
                    return BadRequest(response);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user with email: {Email}", email);
                return StatusCode(500, new ResponseDto<bool>
                {
                    IsSuccess = false,
                    Message = $"Error deleting user: {ex.Message}"
                });
            }
        }

        [HttpPost("forgot-password")]
        [SwaggerOperation(
        Summary = "Quên mật khẩu")]
        public async Task<ActionResult<ResponseDto<bool>>> ForgotPassword([FromBody] ForgotPasswordDTO forgotPassword)
        {
            _logger.LogInformation("Attempting to send forgot password OTP to email: {Email}", forgotPassword.Email);
            var response = await _authService.ForgotPasswordAsync(forgotPassword);
            
            if (!response.IsSuccess)
            {
                _logger.LogWarning("Failed to send forgot password OTP: {Message}", response.Message);
                return BadRequest(response);
            }

            _logger.LogInformation("Successfully sent forgot password OTP to email: {Email}", forgotPassword.Email);
            return Ok(response);
        }

        [HttpPost("reset-password")]
        [SwaggerOperation(
        Summary = "Reset mật khẩu")]
        public async Task<ActionResult<ResponseDto<bool>>> ResetPassword([FromBody] ResetPasswordDTO resetPassword)
        {
            _logger.LogInformation("Attempting to reset password for email: {Email}", resetPassword.Email);
            var response = await _authService.ResetPasswordAsync(resetPassword);
            
            if (!response.IsSuccess)
            {
                _logger.LogWarning("Failed to reset password: {Message}", response.Message);
                return BadRequest(response);
            }

            _logger.LogInformation("Successfully reset password for email: {Email}", resetPassword.Email);
            return Ok(response);
        }

        [HttpPost("change-password")]
        [SwaggerOperation(
        Summary = "Đổi mật khẩu")]
        public async Task<ActionResult<ResponseDto<bool>>> ChangePassword([FromBody] ChangePasswordDTO changePassword)
        {
            _logger.LogInformation("Attempting to change password for email: {Email}", changePassword.Email);
            var response = await _authService.ChangePasswordAsync(changePassword);
            
            if (!response.IsSuccess)
            {
                _logger.LogWarning("Failed to change password: {Message}", response.Message);
                return BadRequest(response);
            }

            _logger.LogInformation("Successfully changed password for email: {Email}", changePassword.Email);
            return Ok(response);
        }
    }
} 