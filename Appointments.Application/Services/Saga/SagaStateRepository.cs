using Appointments.Infrastructure.Data;
using Appointments.Infrastructure.Models.Saga;
using Microsoft.EntityFrameworkCore;

namespace Appointments.Application.Services.Saga
{
    public class SagaStateRepository : ISagaStateRepository
    {
        private readonly SagaDbContext _context;

        public SagaStateRepository(SagaDbContext context)
        {
            _context = context;
        }

        public async Task<SagaState?> GetSagaStateAsync(string sagaId)
        {
            return await _context.SagaStates
                .FirstOrDefaultAsync(s => s.Id == sagaId);
        }

        public async Task<SagaState?> GetSagaStateByEntityAsync(string sagaType, string entityId)
        {
            return await _context.SagaStates
                .FirstOrDefaultAsync(s => s.SagaType == sagaType && s.EntityId == entityId);
        }

        public async Task<SagaState> CreateSagaStateAsync(SagaState sagaState)
        {
            sagaState.CreatedAt = DateTime.UtcNow;
            sagaState.LastUpdatedAt = DateTime.UtcNow;
            
            _context.SagaStates.Add(sagaState);
            await _context.SaveChangesAsync();
            return sagaState;
        }

        public async Task<SagaState> UpdateSagaStateAsync(SagaState sagaState)
        {
            sagaState.LastUpdatedAt = DateTime.UtcNow;
            
            _context.SagaStates.Update(sagaState);
            await _context.SaveChangesAsync();
            return sagaState;
        }

        public async Task<bool> DeleteSagaStateAsync(string sagaId)
        {
            var sagaState = await _context.SagaStates.FindAsync(sagaId);
            if (sagaState == null) return false;

            _context.SagaStates.Remove(sagaState);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<SagaState>> GetSagaStatesByTypeAsync(string sagaType)
        {
            return await _context.SagaStates
                .Where(s => s.SagaType == sagaType)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<SagaState>> GetFailedSagaStatesAsync()
        {
            return await _context.SagaStates
                .Where(s => s.State == "Failed")
                .OrderByDescending(s => s.FailedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<SagaState>> GetIncompleteSagaStatesAsync()
        {
            return await _context.SagaStates
                .Where(s => s.State != "Completed" && s.State != "Failed")
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
        }
    }
}
