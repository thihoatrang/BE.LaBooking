using System.ComponentModel.DataAnnotations;

namespace Appointments.Infrastructure.Models.Saga
{
    public class SagaState
    {
        [Key]
        public string Id { get; set; } = string.Empty;
        
        public string SagaType { get; set; } = string.Empty;
        
        public string EntityId { get; set; } = string.Empty;
        
        public string State { get; set; } = string.Empty;
        
        public string Data { get; set; } = string.Empty;
        
        public string? ErrorMessage { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        public DateTime? CompletedAt { get; set; }
        
        public DateTime? FailedAt { get; set; }
        
        public DateTime LastUpdatedAt { get; set; }
    }
}
