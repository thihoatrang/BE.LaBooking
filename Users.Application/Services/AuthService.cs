using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Users.Infrastructure.Data;
using Users.Infrastructure.Models;
using Users.Infrastructure.Models.Dtos;
using Users.Application.Services.IService;
using Microsoft.Extensions.Configuration;

namespace Users.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserDbContext _context;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;
        private static readonly Dictionary<string, (string Otp, DateTime Expiry, RegisterRequestDTO RegisterData)> _otpStore = new();

        public AuthService(
            UserDbContext context, 
            IMapper mapper, 
            IConfiguration configuration,
            IEmailService emailService)
        {
            _context = context;
            _mapper = mapper;
            _configuration = configuration;
            _emailService = emailService;
        }

            public async Task<AuthResponseDTO> LoginAsync(LoginRequestDTO loginRequest)
            {
                var response = new AuthResponseDTO();

                try
                {
                    var user = await _context.Users
                        .FirstOrDefaultAsync(u => u.Email.ToLower() == loginRequest.Email.ToLower());

                    if (user == null)
                    {
                        response.IsSuccess = false;
                        response.Message = "User not found";
                        return response;
                    }

                    bool isPasswordValid = false;
                    try
                    {
                        // Try to verify with BCrypt
                        isPasswordValid = BCrypt.Net.BCrypt.Verify(loginRequest.Password, user.Password);
                    }
                    catch
                    {
                        // If BCrypt verification fails, check if it's a legacy password
                        isPasswordValid = user.Password == loginRequest.Password;
                    
                        // If it's a legacy password, update it to BCrypt
                        if (isPasswordValid)
                        {
                            user.Password = BCrypt.Net.BCrypt.HashPassword(loginRequest.Password);
                            await _context.SaveChangesAsync();
                        }
                    }

                    if (!isPasswordValid)
                    {
                        response.IsSuccess = false;
                        response.Message = "Invalid password";
                        return response;
                    }

                    var token = GenerateJwtToken(user);
                    response.IsSuccess = true;
                    response.Message = "Login successful";
                    response.Token = token;
                    response.User = _mapper.Map<UserDTO>(user);
                }
                catch (Exception ex)
                {
                    response.IsSuccess = false;
                    response.Message = ex.Message;
                }

                return response;
            }

        public async Task<AuthResponseDTO> RegisterAsync(RegisterRequestDTO registerRequest)
        {
            var response = new AuthResponseDTO();

            try
            {
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == registerRequest.Email.ToLower());

                if (existingUser != null)
                {
                    response.IsSuccess = false;
                    response.Message = "Email already exists";
                    return response;
                }

                // Create user with IsActive = false
                var user = new User
                {
                    Email = registerRequest.Email,
                    FullName = registerRequest.FullName,
                    Password = BCrypt.Net.BCrypt.HashPassword(registerRequest.Password),
                    PhoneNumber = registerRequest.PhoneNumber,
                    Role = registerRequest.Role,
                    IsActive = false
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Generate OTP
                var otp = GenerateOtp();
                var expiry = DateTime.UtcNow.AddMinutes(5);

                // Store OTP
                _otpStore[registerRequest.Email] = (otp, expiry, null);

                // Send OTP email
                await _emailService.SendOtpEmailAsync(registerRequest.Email, otp, "registration");

                response.IsSuccess = true;
                response.Message = "OTP sent to your email. Please verify to activate your account.";
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }

            return response;
        }

        public async Task<AuthResponseDTO> VerifyOtpAsync(VerifyOtpDTO verifyOtp)
        {
            var response = new AuthResponseDTO();

            try
            {
                if (!_otpStore.TryGetValue(verifyOtp.Email, out var otpData))
                {
                    response.IsSuccess = false;
                    response.Message = "OTP not found or expired";
                    return response;
                }

                if (DateTime.UtcNow > otpData.Expiry)
                {
                    _otpStore.Remove(verifyOtp.Email);
                    response.IsSuccess = false;
                    response.Message = "OTP has expired";
                    return response;
                }

                if (otpData.Otp != verifyOtp.Otp)
                {
                    response.IsSuccess = false;
                    response.Message = "Invalid OTP";
                    return response;
                }

                // OTP is valid, activate user
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == verifyOtp.Email.ToLower());

                if (user == null)
                {
                    response.IsSuccess = false;
                    response.Message = "User not found";
                    return response;
                }

                try
                {
                    user.IsActive = true;
                    await _context.SaveChangesAsync();

                    // Remove used OTP
                    _otpStore.Remove(verifyOtp.Email);

                    // Generate JWT token
                    var token = GenerateJwtToken(user);

                    response.IsSuccess = true;
                    response.Message = "Account activated successfully";
                    response.Token = token;
                    response.User = _mapper.Map<UserDTO>(user);
                }
                catch (Exception ex)
                {
                    // If token generation fails, rollback the activation
                    user.IsActive = false;
                    await _context.SaveChangesAsync();
                    throw new Exception($"Failed to activate account: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }

            return response;
        }

        public async Task<ResponseDto<UserDTO>> UpdateUserAsync(int userId, UpdateUserDTO updateUserDto)
        {
            var response = new ResponseDto<UserDTO>();

            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    response.IsSuccess = false;
                    response.Message = "User not found";
                    return response;
                }

                // Check if email is being changed and if it's already taken
                if (user.Email.ToLower() != updateUserDto.Email.ToLower())
                {
                    var existingUser = await _context.Users
                        .FirstOrDefaultAsync(u => u.Email.ToLower() == updateUserDto.Email.ToLower());
                    
                    if (existingUser != null)
                    {
                        response.IsSuccess = false;
                        response.Message = "Email is already taken";
                        return response;
                    }
                }

                // Update user properties
                user.FullName = updateUserDto.FullName;
                user.Email = updateUserDto.Email;
                user.PhoneNumber = updateUserDto.PhoneNumber;
                user.IsActive = updateUserDto.IsActive;

                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                response.Result = _mapper.Map<UserDTO>(user);
                response.Message = "User updated successfully";
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }

            return response;
        }

        private string GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var key = Encoding.ASCII.GetBytes(jwtSettings["SecretKey"]);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["ExpiryInMinutes"])),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature),
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"]
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }

        private string GenerateOtp()
        {
            // Generate a 6-digit OTP
            var random = new Random();
            return random.Next(100000, 999999).ToString();
        }

        public async Task<AuthResponseDTO> GoogleLoginAsync(GoogleAuthDTO googleAuth)
        {
            try
            {
                // Check if user exists with the provided email
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == googleAuth.Email);

                if (user == null)
                {
                    // Create new user if doesn't exist
                    user = new User
                    {
                        Email = googleAuth.Email,
                        FullName = googleAuth.Name, // Use name from Google
                        Password = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString()), // Random password for Google users
                        Role = "Customer",
                        PhoneNumber = "", // Empty phone number
                        IsActive = true,
                        // Nếu User có trường Avatar, thêm dòng sau:
                        // Avatar = googleAuth.Picture
                    };

                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();
                }
                // Nếu User có trường Avatar, cập nhật avatar nếu khác null
                // if (!string.IsNullOrEmpty(googleAuth.Picture))
                // {
                //     user.Avatar = googleAuth.Picture;
                //     await _context.SaveChangesAsync();
                // }
                // Generate JWT token
                var token = GenerateJwtToken(user);

                return new AuthResponseDTO
                {
                    IsSuccess = true,
                    Message = "Google login successful",
                    Token = token,
                    User = new UserDTO
                    {
                        Id = user.Id,
                        FullName = user.FullName,
                        Email = user.Email,
                        PhoneNumber = user.PhoneNumber,
                        Role = user.Role,
                        IsActive = user.IsActive
                    }
                };
            }
            catch (Exception ex)
            {
                return new AuthResponseDTO
                {
                    IsSuccess = false,
                    Message = $"Google login failed: {ex.Message}"
                };
            }
        }

        public async Task<ResponseDto<bool>> DeleteUserAsync(string email)
        {
            var response = new ResponseDto<bool>();

            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());

                if (user == null)
                {
                    response.IsSuccess = false;
                    response.Message = "User not found";
                    return response;
                }

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                response.IsSuccess = true;
                response.Message = "User deleted successfully";
                response.Result = true;
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }

            return response;
        }

        public async Task<ResponseDto<bool>> ForgotPasswordAsync(ForgotPasswordDTO forgotPassword)
        {
            var response = new ResponseDto<bool>();

            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == forgotPassword.Email.ToLower());

                if (user == null)
                {
                    response.IsSuccess = false;
                    response.Message = "User not found";
                    return response;
                }

                // Generate OTP
                var otp = GenerateOtp();
                var expiry = DateTime.UtcNow.AddMinutes(5);

                // Store OTP
                _otpStore[forgotPassword.Email] = (otp, expiry, null);

                // Send OTP email
                await _emailService.SendOtpEmailAsync(forgotPassword.Email, otp, "forgotpassword");

                response.IsSuccess = true;
                response.Message = "OTP sent to your email";
                response.Result = true;
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }

            return response;
        }

        public async Task<ResponseDto<bool>> ResetPasswordAsync(ResetPasswordDTO resetPassword)
        {
            var response = new ResponseDto<bool>();

            try
            {
                if (!_otpStore.TryGetValue(resetPassword.Email, out var otpData))
                {
                    response.IsSuccess = false;
                    response.Message = "OTP not found or expired";
                    return response;
                }

                if (DateTime.UtcNow > otpData.Expiry)
                {
                    _otpStore.Remove(resetPassword.Email);
                    response.IsSuccess = false;
                    response.Message = "OTP has expired";
                    return response;
                }

                if (otpData.Otp != resetPassword.Otp)
                {
                    response.IsSuccess = false;
                    response.Message = "Invalid OTP";
                    return response;
                }

                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == resetPassword.Email.ToLower());

                if (user == null)
                {
                    response.IsSuccess = false;
                    response.Message = "User not found";
                    return response;
                }

                // Update password
                user.Password = BCrypt.Net.BCrypt.HashPassword(resetPassword.NewPassword);
                await _context.SaveChangesAsync();

                // Remove used OTP
                _otpStore.Remove(resetPassword.Email);

                response.IsSuccess = true;
                response.Message = "Password reset successfully";
                response.Result = true;
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }

            return response;
        }

        public async Task<ResponseDto<bool>> ChangePasswordAsync(ChangePasswordDTO changePassword)
        {
            var response = new ResponseDto<bool>();

            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == changePassword.Email.ToLower());

                if (user == null)
                {
                    response.IsSuccess = false;
                    response.Message = "User not found";
                    return response;
                }

                // Verify current password
                bool isPasswordValid = BCrypt.Net.BCrypt.Verify(changePassword.CurrentPassword, user.Password);
                if (!isPasswordValid)
                {
                    response.IsSuccess = false;
                    response.Message = "Current password is incorrect";
                    return response;
                }

                // Update password
                user.Password = BCrypt.Net.BCrypt.HashPassword(changePassword.NewPassword);
                await _context.SaveChangesAsync();

                response.IsSuccess = true;
                response.Message = "Password changed successfully";
                response.Result = true;
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }

            return response;
        }

        public Task<UserDTO?> GetUserByEmailAsync(string email)
        {
            throw new NotImplementedException();
        }
    }
} 