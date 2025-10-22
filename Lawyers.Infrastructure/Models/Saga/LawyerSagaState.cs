using Lawyers.Infrastructure.Models;

namespace Lawyers.Infrastructure.Models.Saga
{
    public enum LawyerSagaState
    {
        Started,
        ProfileCreated,
        WorkSlotsCreated,
        EmailSent,
        Completed,
        Failed,
        Compensating
    }

    public class LawyerSagaData
    {
        public int LawyerId { get; set; }
        public int UserId { get; set; }
        public string Bio { get; set; } = string.Empty;
        public string LicenseNum { get; set; } = string.Empty;
        public int ExpYears { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Rating { get; set; }
        public decimal PricePerHour { get; set; }
        public string Img { get; set; } = string.Empty;
        public string DayOfWeek { get; set; } = string.Empty;
        public string WorkTime { get; set; } = string.Empty;
        public LawyerSagaState State { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public List<int> WorkSlotIds { get; set; } = new();
        public List<int> PracticeAreaIds { get; set; } = new();
    }
}
