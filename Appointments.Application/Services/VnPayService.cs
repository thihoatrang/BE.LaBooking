using System.Security.Cryptography;
using System.Text;
using Appointments.Infrastructure.Models.Dtos;
using Appointments.Application.Services.IService;
using Microsoft.Extensions.Configuration;
using System.Net;

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
            var vnp_OrderType = "240000";
            var vnp_Amount = request.Amount * 100; // in VND x100 per VNPAY rules
            var vnp_Locale = locale;
            var vnp_ReturnUrl = returnUrl;
            var vnp_IpAddr = "128.199.178.23"; // could extract from HttpContext if available
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
            var vnp_SecureHash = HmacSha512(hashSecret, signData);
            // 3️⃣ Tạo query có encode cho URL redirect

            //BuildUrlEncodedQuery(parameters);

            // 4️⃣ Gắn hash cuối cùng
            var paymentUrl = $"{baseUrl}?{signData}&vnp_SecureHashType=HMACSHA512&vnp_SecureHash={vnp_SecureHash}";

            return await Task.FromResult(new CreatePaymentResponseDto
            {
                Vendor = Vendor,
                PaymentUrl = paymentUrl,
                OrderId = request.OrderId
            });
        }

        public bool VerifySignature(IDictionary<string, string> parameters, string signatureParamName)
        {
            try
            {
                var settings = _configuration.GetSection("Payments:VnPay");
                var hashSecret = settings["HashSecret"] ?? string.Empty;
                
                if (string.IsNullOrEmpty(hashSecret))
                {
                    return false;
                }

                // Filter: Only include vnp_ parameters, exclude signature params and empty values
                var sorted = new SortedDictionary<string, string>(
                    parameters
                        .Where(kv => 
                            kv.Key.StartsWith("vnp_", StringComparison.OrdinalIgnoreCase) &&
                            !string.Equals(kv.Key, signatureParamName, StringComparison.OrdinalIgnoreCase) &&
                            !string.Equals(kv.Key, "vnp_SecureHashType", StringComparison.OrdinalIgnoreCase) &&
                            !string.IsNullOrEmpty(kv.Value))
                        .ToDictionary(k => k.Key, v => v.Value), 
                    StringComparer.Ordinal);

                // Use the same format as BuildRawStringForSign for verification
                // VNPay uses URL encoding with %20 replaced by +
                // Note: Values from Request.Query are already decoded by ASP.NET Core,
                // so we need to encode them again to match the format used when creating payment URL
                string FromEncode(string value) => WebUtility.UrlEncode(value).Replace("%20", "+");
                var query = string.Join("&", sorted
                    .Where(kv => !string.IsNullOrEmpty(kv.Value))
                    .Select(kv => $"{kv.Key}={FromEncode(kv.Value)}"));

                var computed = HmacSha512(hashSecret, query);
                var provided = parameters.TryGetValue(signatureParamName, out var sig) ? sig : string.Empty;
                
                var isMatch = string.Equals(computed, provided, StringComparison.OrdinalIgnoreCase);
                
                // Log for debugging
                if (!isMatch)
                {
                    System.Diagnostics.Debug.WriteLine($"Signature mismatch - Computed: {computed}, Provided: {provided}");
                    System.Diagnostics.Debug.WriteLine($"Query string: {query}");
                }
                
                return isMatch;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Signature verification error: {ex.Message}");
                return false;
            }
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

        private string HmacSha512(string secret, string data)
        {
            using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(secret));
            var bytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
            var sb = new StringBuilder(bytes.Length * 2);
            foreach (var b in bytes)
            {
                sb.Append(b.ToString("X2"));
            }
            return sb.ToString();
        }
        private static string BuildRawStringForSign(IDictionary<string, string> parameters)
        {
            string FromEncode(string enUrl) => WebUtility.UrlEncode(enUrl).Replace("%20", "+");
            return string.Join("&", parameters
                .Where(kv => !string.IsNullOrEmpty(kv.Value))
                .Select(kv => $"{kv.Key}={FromEncode(kv.Value)}"));
        }
    }
}


