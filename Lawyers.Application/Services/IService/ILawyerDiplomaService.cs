using Lawyers.Infrastructure.Models.Dtos;

namespace Lawyer.Application.Services.IService
{
    public interface ILawyerDiplomaService
    {
        Task<ResponseDto<IEnumerable<LawyerDiplomaDTO>>> GetAllDiplomasAsync(bool includeDeleted = false);
        Task<ResponseDto<LawyerDiplomaDTO>> GetDiplomaByIdAsync(int id);
        Task<ResponseDto<IEnumerable<LawyerDiplomaDTO>>> GetDiplomasByLawyerIdAsync(int lawyerId, bool includeDeleted = false);
        Task<ResponseDto<LawyerDiplomaDTO>> CreateDiplomaAsync(int lawyerId, LawyerDiplomaCreateDto diplomaDto);
        Task<ResponseDto<LawyerDiplomaDTO>> UpdateDiplomaAsync(int id, LawyerDiplomaUpdateDto diplomaDto);
        Task<ResponseDto<bool>> DeleteDiplomaAsync(int id);
        Task<ResponseDto<bool>> DeleteDiplomaHardAsync(int id);
    }
} 