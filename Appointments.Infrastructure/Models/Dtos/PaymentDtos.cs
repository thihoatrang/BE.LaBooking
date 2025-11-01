using System.ComponentModel.DataAnnotations;

namespace Appointments.Infrastructure.Models.Dtos
{
    public class CreatePaymentRequestDto
    {
        [Required]
        public string Vendor { get; set; } = string.Empty; // vnpay | momo

        [Required]
        public string OrderId { get; set; } = string.Empty; // appointment id or generated order id

        [Required]
        public long Amount { get; set; } // in VND, integer

        public string? OrderInfo { get; set; }
        public string? ReturnUrl { get; set; }
        public string? IpnUrl { get; set; }
    }

    public class CreatePaymentForAppointmentRequestDto
    {
        [Required]
        public string Vendor { get; set; } = string.Empty; // vnpay | momo

        [Required]
        public string OrderId { get; set; } = string.Empty; // appointment id or generated order id

        [Required]
        public int LawyerId { get; set; }

        [Required]
        public int DurationHours { get; set; } = 1; // Default 1 hour

        public int? AppointmentId { get; set; } // Link to appointment

        public string? OrderInfo { get; set; }
        public string? ReturnUrl { get; set; }
        public string? IpnUrl { get; set; }
    }

    public class CreatePaymentResponseDto
    {
        public string Vendor { get; set; } = string.Empty;
        public string PaymentUrl { get; set; } = string.Empty; // redirect/payUrl
        public string? QrImageBase64 { get; set; } // optional
        public string OrderId { get; set; } = string.Empty;
    }

    public class PaymentStatusDto
    {
        public string OrderId { get; set; } = string.Empty;
        public string Vendor { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; // success | failed | pending
        public string? Message { get; set; }
        public string? TransactionId { get; set; }
    }

    // VNPAY IPN/Return parameters
    public class VnPayCallbackDto
    {
        public Dictionary<string, string> Query { get; set; } = new();
    }
}


