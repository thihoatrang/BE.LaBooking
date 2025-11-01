namespace Users.Infrastructure.Models.Dtos
{
    public class DashboardStatisticsDto
    {
        public PaymentStatisticsDto Payments { get; set; } = new();
        public AppointmentStatisticsDto Appointments { get; set; } = new();
        public ReviewStatisticsDto Reviews { get; set; } = new();
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }

    public class PaymentStatisticsDto
    {
        public int TotalCount { get; set; }
        public long TotalAmount { get; set; }
        public Dictionary<string, int> CountByStatus { get; set; } = new();
        public Dictionary<string, int> CountByVendor { get; set; } = new();
        public Dictionary<string, long> AmountByStatus { get; set; } = new();
        public List<DailyStatisticsDto> DailyStatistics { get; set; } = new();
    }

    public class AppointmentStatisticsDto
    {
        public int TotalCount { get; set; }
        public Dictionary<string, int> CountByStatus { get; set; } = new();
        public List<DailyStatisticsDto> DailyStatistics { get; set; } = new();
        public int ActiveAppointments { get; set; }
        public int CompletedAppointments { get; set; }
        public int PendingAppointments { get; set; }
        public int PaymentPendingAppointments { get; set; }
        public int CancelledAppointments { get; set; }
    }

    public class ReviewStatisticsDto
    {
        public int TotalCount { get; set; }
        public decimal AverageRating { get; set; }
        public Dictionary<int, int> RatingDistribution { get; set; } = new(); // Key: Rating (1-5), Value: Count
        public List<DailyStatisticsDto> DailyStatistics { get; set; } = new();
    }

    public class DailyStatisticsDto
    {
        public DateTime Date { get; set; }
        public int Count { get; set; }
        public long? Amount { get; set; } // Only for payments
    }
}

