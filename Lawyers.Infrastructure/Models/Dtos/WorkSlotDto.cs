namespace Lawyers.Infrastructure.Models.Dtos
{
    public class WorkSlotDto
    {
        public int Id { get; set; }
        public int LawyerId { get; set; }
        public string DayOfWeek { get; set; }
        public string Slot { get; set; }
        public bool IsActive { get; set; }
    }
} 