namespace Appointments.Infrastructure.Models.DTOs
{
    public class LawyerProfileDTO
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public string Bio { get; set; }

        public List<string> Spec { get; set; }

        public string LicenseNum { get; set; }

        public int ExpYears { get; set; }

        public string Description { get; set; }

        public double Rating { get; set; }

        public double PricePerHour { get; set; }

        public string Img { get; set; }

        public string DayOfWeek { get; set; }

        public string WorkTime { get; set; }
    }
}
