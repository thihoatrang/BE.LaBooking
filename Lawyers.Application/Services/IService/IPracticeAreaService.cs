using Lawyers.Infrastructure.Models.Dtos;

namespace Lawyers.Application.Services.IService
{
    public interface IPracticeAreaService
    {
        Task<IEnumerable<PracticeAreaDTO>> GetAllPracticeAreasAsync();
        Task<PracticeAreaDTO?> GetPracticeAreaByIdAsync(int id);
        Task<PracticeAreaDTO?> GetPracticeAreaByCodeAsync(string code);
        Task<PracticeAreaDTO> CreatePracticeAreaAsync(PracticeAreaCreateDTO dto);
        Task<bool> UpdatePracticeAreaAsync(int id, PracticeAreaUpdateDTO dto);
        Task<bool> DeletePracticeAreaAsync(int id);
    }
}
