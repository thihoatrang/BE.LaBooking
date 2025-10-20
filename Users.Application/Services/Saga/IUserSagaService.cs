using Users.Infrastructure.Models.Dtos;
using Users.Infrastructure.Models.Saga;

namespace Users.Application.Services.Saga
{
    public interface IUserSagaService
    {
        Task<UserSagaData> StartUserRegistrationSagaAsync(RegisterRequestDTO dto);
        Task<UserSagaData> StartUserCreationSagaAsync(UserDTO dto);
        Task<bool> CompleteSagaAsync(int userId);
        Task<bool> CompensateSagaAsync(int userId, string reason);
        Task<UserSagaData?> GetSagaStateAsync(int userId);
        Task<bool> UpdateSagaStateAsync(int userId, UserSagaState newState, string? errorMessage = null);
        Task SendNotificationEmailAsync(int userId);
    }
}
