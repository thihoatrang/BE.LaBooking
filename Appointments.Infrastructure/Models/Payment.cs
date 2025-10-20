using System.ComponentModel.DataAnnotations;

namespace Appointments.Infrastructure.Models
{
    public class Payment
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string OrderId { get; set; } = string.Empty;
        [Required]
        public string Vendor { get; set; } = string.Empty; // vnpay|momo
        public long Amount { get; set; }
        [Required]
        public string Status { get; set; } = "pending"; // pending|success|failed
        public string? TransactionId { get; set; }
        public string? Message { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}


