namespace Appointments.Infrastructure.Models.Dtos
{
    public class CreateAppointmentDTO
    {
        public int UserId { get; set; }
        public int LawyerId { get; set; }
        public DateTime ScheduledAt { get; set; }
        public string Slot { get; set; }
        public string? Note { get; set; }
        public string Spec { get; set; }
        public List<string> Services { get; set; }
    }
}
