using Users.Infrastructure.Models.Dtos;

namespace Users.Application.Services.IService
{
    public interface IUserWithLawyerProfileService
    {
        Task<IEnumerable<UserWithLawyerProfileDTO>> GetAllUsersWithLawyerProfileAsync();
        Task<UserWithLawyerProfileDTO?> GetUsersWithLawyerProfileByIdAsync(int userId);
        Task<IEnumerable<UserWithLawyerProfileDTO>> GetUsersWithLawyerProfileOnlyAsync();
        Task<IEnumerable<UserWithLawyerProfileDTO>> GetAllUsersWithLawyerProfileIncludingInactiveAsync();
        Task<ResponseDto<bool>> UpdateUserWithLawyerProfileAsync(int userId, UpdateUserWithLawyerProfileDTO dto);
    }
}
