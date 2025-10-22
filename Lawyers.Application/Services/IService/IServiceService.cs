using Lawyers.Infrastructure.Models.Dtos;

namespace Lawyers.Application.Services.IService
{
    public interface IServiceService
    {
        Task<IEnumerable<ServiceDTO>> GetAllServicesAsync();
        Task<ServiceDTO?> GetServiceByIdAsync(int id);
        Task<IEnumerable<ServiceDTO>> GetServicesByPracticeAreaIdAsync(int practiceAreaId);
        Task<ServiceDTO?> GetServiceByCodeAsync(string code);
        Task<ServiceDTO> CreateServiceAsync(ServiceCreateDTO dto);
        Task<bool> UpdateServiceAsync(int id, ServiceUpdateDTO dto);
        Task<bool> DeleteServiceAsync(int id);
    }
}
