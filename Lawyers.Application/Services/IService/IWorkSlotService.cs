using Lawyers.Infrastructure.Models.Dtos;

namespace Lawyer.Application.Services.IService
{
    public interface IWorkSlotService
    {
        Task<IEnumerable<WorkSlotDto>> GetAllWorkSlotsAsync();
        Task<WorkSlotDto> GetWorkSlotByIdAsync(int id);
        Task<WorkSlotDto> CreateWorkSlotAsync(int lawyerId, CreateWorkSlotDto createWorkSlotDto);
        Task<WorkSlotDto> UpdateWorkSlotAsync(int lawyerId, UpdateWorkSlotDtoNoId updateWorkSlotDto, int workSlotId);
        Task<bool> DeleteWorkSlotAsync(int id);
        Task<IEnumerable<WorkSlotDto>> GetWorkSlotsByLawyerIdAsync(int lawyerId);
        Task<bool> DeactivateWorkSlotAsync(DeactivateWorkSlotDto dto);
        Task<bool> ActivateWorkSlotAsync(ActivateWorkSlotDto dto);
        Task<IEnumerable<WorkSlotDto>> GetAllWorkSlotsAsync(bool includeInactive = false);
    }
} 