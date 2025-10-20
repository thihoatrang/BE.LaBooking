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

    // MoMo IPN body
    public class MoMoIpnDto
    {
        public string partnerCode { get; set; } = string.Empty;
        public string orderId { get; set; } = string.Empty;
        public string requestId { get; set; } = string.Empty;
        public long amount { get; set; }
        public string orderInfo { get; set; } = string.Empty;
        public string orderType { get; set; } = string.Empty;
        public long transId { get; set; }
        public int resultCode { get; set; }
        public string message { get; set; } = string.Empty;
        public string payType { get; set; } = string.Empty;
        public long responseTime { get; set; }
        public string extraData { get; set; } = string.Empty;
        public string signature { get; set; } = string.Empty;
    }
}


