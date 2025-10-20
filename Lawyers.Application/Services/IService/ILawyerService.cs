using Lawyers.Infrastructure.Models;
using Lawyers.Infrastructure.Models.Dtos;

namespace Lawyer.Application.Services.IService
{
    public interface ILawyerService
    {
        Task<IEnumerable<LawyerProfileDTO>> GetAllLawyersAsync();
        Task<LawyerProfileDTO?> GetLawyerByIdAsync(int id);
        Task<LawyerProfileDTO> CreateLawyerAsync(LawyerProfileDTO profileDto);
        Task<LawyerProfileDTO?> GetLawyerByUserIdAsync(int userId);
        Task<bool> UpdateLawyerAsync(int id, LawyerProfileDTO profileDto);
        Task<bool> DeleteLawyerAsync(int id);

        Task<LawyerProfileDTO?> UpdateLawyerProfileAsync(int id, UpdateLawyerDTO dto);
    }
}
