using Appointments.Infrastructure.Models.Enums;

namespace Appointments.Infrastructure.Models.Saga
{
    public enum AppointmentSagaState
    {
        Started,
        WorkSlotDeactivated,
        EmailSent,
        Completed,
        Failed,
        Compensating
    }

    public class AppointmentSagaData
    {
        public int AppointmentId { get; set; }
        public int UserId { get; set; }
        public int LawyerId { get; set; }
        public DateTime ScheduledAt { get; set; }
        public string Slot { get; set; } = string.Empty;
        public string Spec { get; set; } = string.Empty;
        public List<string> Services { get; set; } = new();
        public string Note { get; set; } = string.Empty;
        public AppointmentSagaState State { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
    }
}
