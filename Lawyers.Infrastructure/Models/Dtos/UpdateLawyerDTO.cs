namespace Lawyers.Infrastructure.Models.Dtos
{
    public class UpdateLawyerDTO
    {
        public string? Bio { get; set; }

        public string LicenseNum { get; set; }

        public int ExpYears { get; set; }

        public string? Description { get; set; }

        public double Rating { get; set; }

        public double PricePerHour { get; set; }

        public string? Img { get; set; }

        public string DayOfWeek { get; set; }

        public string WorkTime { get; set; }

        public List<int> PracticeAreaIds { get; set; } = new List<int>();
    }
}
