using Appointments.Infrastructure.Models.Saga;

namespace Appointments.Application.Services.Saga
{
    public interface ISagaStateRepository
    {
        Task<SagaState?> GetSagaStateAsync(string sagaId);
        Task<SagaState?> GetSagaStateByEntityAsync(string sagaType, string entityId);
        Task<SagaState> CreateSagaStateAsync(SagaState sagaState);
        Task<SagaState> UpdateSagaStateAsync(SagaState sagaState);
        Task<bool> DeleteSagaStateAsync(string sagaId);
        Task<IEnumerable<SagaState>> GetSagaStatesByTypeAsync(string sagaType);
        Task<IEnumerable<SagaState>> GetFailedSagaStatesAsync();
        Task<IEnumerable<SagaState>> GetIncompleteSagaStatesAsync();
    }
}
