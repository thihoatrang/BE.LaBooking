using Users.Infrastructure.Models;
using Users.Infrastructure.Models.Dtos;
using Users.Infrastructure.Models.Saga;
using Users.Infrastructure.Repository;
using Users.Application.Services.IService;
using Microsoft.Extensions.Logging;

namespace Users.Application.Services.Saga
{
    public class UserSagaService : IUserSagaService
    {
        private readonly IUserRepository _userRepository;
        private readonly IEmailService _emailService;
        private readonly IAuthService _authService;
        private readonly ILogger<UserSagaService> _logger;
        private readonly Dictionary<int, UserSagaData> _sagaStates = new();

        public UserSagaService(
            IUserRepository userRepository,
            IEmailService emailService,
            IAuthService authService,
            ILogger<UserSagaService> logger)
        {
            _userRepository = userRepository;
            _emailService = emailService;
            _authService = authService;
            _logger = logger;
        }

        public async Task<UserSagaData> StartUserRegistrationSagaAsync(RegisterRequestDTO dto)
        {
            var sagaData = new UserSagaData
            {
                Email = dto.Email,
                FullName = dto.FullName,
                State = UserSagaState.Started,
                CreatedAt = DateTime.UtcNow
            };

            try
            {
                // Step 1: Đăng ký user (AuthService lo gửi OTP)
                var registerResp = await _authService.RegisterAsync(dto);
                if (!registerResp.IsSuccess)
                    throw new Exception(registerResp.Message);

                var user = await _userRepository.GetByEmailAsync(dto.Email);
                sagaData.UserId = user.Id;

                _sagaStates[user.Id] = sagaData;
                _logger.LogInformation($"Saga: User {user.Email} registered, OTP sent");

                // Step 2: Hoàn tất Saga
                await CompleteSagaAsync(user.Id);
                return sagaData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Saga failed for {dto.Email}");
                if (sagaData.UserId > 0)
                    await CompensateSagaAsync(sagaData.UserId, ex.Message);

                sagaData.State = UserSagaState.Failed;
                sagaData.ErrorMessage = ex.Message;
                throw;
            }
        }


        public async Task<UserSagaData> StartUserCreationSagaAsync(UserDTO dto)
        {
            var sagaData = new UserSagaData
            {
                Email = dto.Email,
                FullName = dto.FullName,
                PhoneNumber = dto.PhoneNumber,
                Role = dto.Role,
                State = UserSagaState.Started,
                CreatedAt = DateTime.UtcNow,
                IsLawyer = dto.Role?.ToLower() == "lawyer"
            };

            try
            {
                // Step 1: Create user
                var user = new User
                {
                    Email = dto.Email,
                    FullName = dto.FullName,
                    PhoneNumber = dto.PhoneNumber,
                    Role = dto.Role,
                    IsActive = dto.IsActive
                };

                await _userRepository.AddAsync(user);
                sagaData.UserId = user.Id;
                _sagaStates[user.Id] = sagaData;

                _logger.LogInformation($"User creation saga started for user {user.Id}");

                // Step 2: Send notification email
                await SendNotificationEmailAsync(user.Id);

                // Step 3: Complete saga
                await CompleteSagaAsync(user.Id);

                return sagaData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"User creation saga failed for user {sagaData.UserId}");
                sagaData.State = UserSagaState.Failed;
                sagaData.ErrorMessage = ex.Message;
                
                if (sagaData.UserId > 0)
                {
                    await CompensateSagaAsync(sagaData.UserId, ex.Message);
                }
                
                throw;
            }
        }

        private async Task SendNotificationEmailAsync(int userId)
        {
            var sagaData = _sagaStates[userId];
            var user = await _userRepository.GetByIdAsync(userId);
            
            if (user == null)
                throw new InvalidOperationException($"User {userId} not found");

            string subject = "[Law Appointment App] Tài khoản mới đã được tạo";
            string htmlBody = $@"
                <h2>Xin chào {user.FullName},</h2>
                <p>Tài khoản của bạn đã được tạo trong hệ thống Law Appointment App.</p>
                <ul>
                    <li><b>Email:</b> {user.Email}</li>
                    <li><b>Vai trò:</b> {user.Role}</li>
                    <li><b>Trạng thái:</b> {(user.IsActive ? "Hoạt động" : "Tạm khóa")}</li>
                </ul>
                <p>Trân trọng,<br/>Law Appointment App</p>";
            
            await _emailService.SendNotificationEmailAsync(user.Email, subject, htmlBody);
            await _emailService.SendNotificationEmailAsync(user.Email, subject, htmlBody);
            sagaData.State = UserSagaState.EmailSent;
            _logger.LogInformation($"Notification email sent for user {userId}");
        }

        public async Task<bool> CompleteSagaAsync(int userId)
        {
            if (!_sagaStates.ContainsKey(userId))
                return false;

            var sagaData = _sagaStates[userId];
            sagaData.State = UserSagaState.Completed;
            sagaData.CompletedAt = DateTime.UtcNow;
            
            _logger.LogInformation($"User saga completed for user {userId}");
            return true;
        }

        public async Task<bool> CompensateSagaAsync(int userId, string reason)
        {
            if (!_sagaStates.ContainsKey(userId))
                return false;

            var sagaData = _sagaStates[userId];
            sagaData.State = UserSagaState.Compensating;
            
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user != null)
                {
                    // Compensate: Soft delete user
                    user.IsActive = false;
                    await _userRepository.UpdateAsync(user);
                }

                sagaData.State = UserSagaState.Failed;
                sagaData.ErrorMessage = reason;
                
                _logger.LogInformation($"User saga compensated for user {userId}: {reason}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to compensate user saga for user {userId}");
                return false;
            }
        }

        public async Task<UserSagaData?> GetSagaStateAsync(int userId)
        {
            return _sagaStates.ContainsKey(userId) ? _sagaStates[userId] : null;
        }

        public async Task<bool> UpdateSagaStateAsync(int userId, UserSagaState newState, string? errorMessage = null)
        {
            if (!_sagaStates.ContainsKey(userId))
                return false;

            var sagaData = _sagaStates[userId];
            sagaData.State = newState;
            if (!string.IsNullOrEmpty(errorMessage))
            {
                sagaData.ErrorMessage = errorMessage;
            }

            return true;
        }

        Task IUserSagaService.SendNotificationEmailAsync(int userId)
        {
            return SendNotificationEmailAsync(userId);
        }
    }
}
