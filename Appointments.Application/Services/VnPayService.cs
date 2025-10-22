using System.Security.Cryptography;
using System.Text;
using Appointments.Infrastructure.Models.Dtos;
using Appointments.Application.Services.IService;
using Microsoft.Extensions.Configuration;

namespace Appointments.Application.Services
{
    public class VnPayService : IPaymentProvider
    {
        private readonly IConfiguration _configuration;

        public VnPayService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string Vendor => "vnpay";

        public async Task<CreatePaymentResponseDto> CreateAsync(CreatePaymentRequestDto request, CancellationToken ct = default)
        {
            var settings = _configuration.GetSection("Payments:VnPay");
            var tmnCode = settings["TmnCode"] ?? string.Empty;
            var hashSecret = settings["HashSecret"] ?? string.Empty;
            var baseUrl = settings["BaseUrl"] ?? string.Empty;
            var returnUrl = request.ReturnUrl ?? settings["ReturnUrl"] ?? string.Empty;
            // var ipnUrl = request.IpnUrl ?? settings["IpnUrl"] ?? string.Empty; // Comment out for local development
            var locale = settings["Locale"] ?? "vn";
            var currCode = settings["CurrCode"] ?? "VND";

            var vnp_Version = "2.1.0";
            var vnp_Command = "pay";
            var vnp_TxnRef = request.OrderId;
            var vnp_OrderInfo = string.IsNullOrEmpty(request.OrderInfo) ? $"Payment for order {request.OrderId}" : request.OrderInfo;
            var vnp_OrderType = "billpayment";
            var vnp_Amount = request.Amount * 100; // in VND x100 per VNPAY rules
            var vnp_Locale = locale;
            var vnp_ReturnUrl = returnUrl;
            var vnp_IpAddr = "113.161.78.90"; // could extract from HttpContext if available
            var vnp_CreateDate = DateTime.Now.ToString("yyyyMMddHHmmss");

            var parameters = new SortedDictionary<string, string>
            {
                ["vnp_Version"] = vnp_Version,
                ["vnp_Command"] = vnp_Command,
                ["vnp_TmnCode"] = tmnCode,
                ["vnp_Amount"] = vnp_Amount.ToString(),
                ["vnp_CurrCode"] = currCode,
                ["vnp_TxnRef"] = vnp_TxnRef,
                ["vnp_OrderInfo"] = vnp_OrderInfo,
                ["vnp_OrderType"] = vnp_OrderType,
                ["vnp_Locale"] = vnp_Locale,
                ["vnp_ReturnUrl"] = vnp_ReturnUrl,
                ["vnp_IpAddr"] = vnp_IpAddr,
                ["vnp_CreateDate"] = vnp_CreateDate
            };

            // Skip IPN URL for local development - VNPay cannot reach localhost
            // if (!string.IsNullOrEmpty(ipnUrl))
            // {
            //     parameters["vnp_IpnUrl"] = ipnUrl;
            // }

            // 1️⃣ Tạo chuỗi raw để ký
            var signData = BuildRawStringForSign(parameters);

            // 2️⃣ Tạo hash
            var vnp_SecureHash = HmacSHA512(hashSecret, signData);

            // 3️⃣ Tạo query có encode cho URL redirect
            var query = BuildUrlEncodedQuery(parameters);

            // 4️⃣ Gắn hash cuối cùng
            var paymentUrl = $"{baseUrl}?{query}&vnp_SecureHash={vnp_SecureHash}";

            return await Task.FromResult(new CreatePaymentResponseDto
            {
                Vendor = Vendor,
                PaymentUrl = paymentUrl,
                OrderId = request.OrderId
            });
        }

        public bool VerifySignature(IDictionary<string, string> parameters, string signatureParamName)
        {
            var settings = _configuration.GetSection("Payments:VnPay");
            var hashSecret = settings["HashSecret"] ?? string.Empty;
            var sorted = new SortedDictionary<string, string>(parameters
                .Where(kv => !string.Equals(kv.Key, signatureParamName, StringComparison.OrdinalIgnoreCase))
                .ToDictionary(k => k.Key, v => v.Value), StringComparer.Ordinal);
            var query = BuildQuery(sorted);
            var computed = HmacSHA512(hashSecret, query);
            var provided = parameters.TryGetValue(signatureParamName, out var sig) ? sig : string.Empty;
            return string.Equals(computed, provided, StringComparison.OrdinalIgnoreCase);
        }

        public PaymentStatusDto ParseCallback(IDictionary<string, string> parameters)
        {
            var status = new PaymentStatusDto
            {
                Vendor = Vendor,
                OrderId = parameters.ContainsKey("vnp_TxnRef") ? parameters["vnp_TxnRef"] : string.Empty,
                TransactionId = parameters.ContainsKey("vnp_TransactionNo") ? parameters["vnp_TransactionNo"] : null
            };

            if (parameters.TryGetValue("vnp_ResponseCode", out var code) && code == "00")
            {
                status.Status = "success";
                status.Message = "Payment successful";
            }
            else
            {
                status.Status = "failed";
                status.Message = parameters.TryGetValue("vnp_Message", out var msg) ? msg : "Payment failed";
            }

            return status;
        }

        private static string BuildQuery(IDictionary<string, string> parameters)
        {
            var query = string.Join("&", parameters
                .Where(kv => !string.IsNullOrEmpty(kv.Value))
                .Select(kv => $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value)}"));
            return query;
        }

        private static string HmacSHA512(string key, string data)
        {
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var dataBytes = Encoding.UTF8.GetBytes(data);
            using var hmac = new HMACSHA512(keyBytes);
            var hash = hmac.ComputeHash(dataBytes);
            return BitConverter.ToString(hash).Replace("-", string.Empty).ToLowerInvariant();
        }

        private static string BuildRawStringForSign(IDictionary<string, string> parameters)
        {
            return string.Join("&", parameters
                .Where(kv => !string.IsNullOrEmpty(kv.Value))
                .Select(kv => $"{kv.Key}={kv.Value}"));
        }

        private static string BuildUrlEncodedQuery(IDictionary<string, string> parameters)
        {
            return string.Join("&", parameters
                .Where(kv => !string.IsNullOrEmpty(kv.Value))
                .Select(kv => $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value)}"));
        }
    }
}


