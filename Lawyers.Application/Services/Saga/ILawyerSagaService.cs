using Lawyers.Infrastructure.Models.Dtos;
using Lawyers.Infrastructure.Models.Saga;

namespace Lawyer.Application.Services.Saga
{
    public interface ILawyerSagaService
    {
        Task<LawyerSagaData> StartLawyerCreationSagaAsync(LawyerProfileDTO dto);
        Task<LawyerSagaData> StartLawyerUpdateSagaAsync(int id, LawyerProfileDTO dto);
        Task<bool> CompleteSagaAsync(int lawyerId);
        Task<bool> CompensateSagaAsync(int lawyerId, string reason);
        Task<LawyerSagaData?> GetSagaStateAsync(int lawyerId);
        Task<bool> UpdateSagaStateAsync(int lawyerId, LawyerSagaState newState, string? errorMessage = null);
    }
}
