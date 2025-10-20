using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using Appointments.Infrastructure.Models.Dtos;
using Appointments.Application.Services.IService;
using Microsoft.Extensions.Configuration;

namespace Appointments.Application.Services
{
    public class MoMoService : IPaymentProvider
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public MoMoService(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _httpClient = httpClientFactory.CreateClient();
        }

        public string Vendor => "momo";

        public async Task<CreatePaymentResponseDto> CreateAsync(CreatePaymentRequestDto request, CancellationToken ct = default)
        {
            var settings = _configuration.GetSection("Payments:MoMo");
            var partnerCode = settings["PartnerCode"] ?? string.Empty;
            var accessKey = settings["AccessKey"] ?? string.Empty;
            var secretKey = settings["SecretKey"] ?? string.Empty;
            var endpoint = settings["Endpoint"] ?? string.Empty;
            var returnUrl = request.ReturnUrl ?? settings["ReturnUrl"] ?? string.Empty;
            var ipnUrl = request.IpnUrl ?? settings["IpnUrl"] ?? string.Empty;

            var orderId = request.OrderId;
            var requestId = Guid.NewGuid().ToString("N");
            var orderInfo = string.IsNullOrEmpty(request.OrderInfo) ? $"Payment for order {orderId}" : request.OrderInfo!;
            var amount = request.Amount;
            var orderType = "momo_wallet";
            var extraData = string.Empty; // base64 if needed

            // rawSignature order
            var raw = $"accessKey={accessKey}&amount={amount}&extraData={extraData}&ipnUrl={ipnUrl}&orderId={orderId}&orderInfo={orderInfo}&partnerCode={partnerCode}&redirectUrl={returnUrl}&requestId={requestId}&requestType=captureWallet";
            var signature = HmacSHA256(secretKey, raw);

            var body = new
            {
                partnerCode,
                partnerName = "LawAppointment",
                storeId = "LAW-STORE",
                requestId,
                amount,
                orderId,
                orderInfo,
                redirectUrl = returnUrl,
                ipnUrl,
                requestType = "captureWallet",
                extraData,
                signature,
                lang = "vi"
            };

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, endpoint)
            {
                Content = JsonContent.Create(body)
            };

            var httpResponse = await _httpClient.SendAsync(httpRequest, ct);
            httpResponse.EnsureSuccessStatusCode();
            var json = await httpResponse.Content.ReadFromJsonAsync<Dictionary<string, object>>(cancellationToken: ct) ?? new();
            var payUrl = json.ContainsKey("payUrl") ? json["payUrl"]?.ToString() ?? string.Empty : string.Empty;

            return new CreatePaymentResponseDto
            {
                Vendor = Vendor,
                PaymentUrl = payUrl,
                OrderId = orderId
            };
        }

        public bool VerifySignature(IDictionary<string, string> parameters, string signatureParamName)
        {
            var settings = _configuration.GetSection("Payments:MoMo");
            var secretKey = settings["SecretKey"] ?? string.Empty;

            // MoMo IPN signature uses a defined field order
            var fields = new[] { "accessKey", "amount", "extraData", "message", "orderId", "orderInfo", "orderType", "partnerCode", "payType", "requestId", "responseTime", "resultCode", "transId" };
            var raw = string.Join("&", fields.Select(f => $"{f}={(parameters.TryGetValue(f, out var v) ? v : string.Empty)}"));
            var computed = HmacSHA256(secretKey, raw);
            var provided = parameters.TryGetValue(signatureParamName, out var sig) ? sig : string.Empty;
            return string.Equals(computed, provided, StringComparison.OrdinalIgnoreCase);
        }

        public PaymentStatusDto ParseCallback(IDictionary<string, string> parameters)
        {
            var status = new PaymentStatusDto
            {
                Vendor = Vendor,
                OrderId = parameters.ContainsKey("orderId") ? parameters["orderId"] : string.Empty,
                TransactionId = parameters.ContainsKey("transId") ? parameters["transId"] : null
            };

            if (parameters.TryGetValue("resultCode", out var codeStr) && int.TryParse(codeStr, out var code) && code == 0)
            {
                status.Status = "success";
                status.Message = parameters.TryGetValue("message", out var msgOk) ? msgOk : "Payment successful";
            }
            else
            {
                status.Status = "failed";
                status.Message = parameters.TryGetValue("message", out var msg) ? msg : "Payment failed";
            }

            return status;
        }

        private static string HmacSHA256(string key, string data)
        {
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var dataBytes = Encoding.UTF8.GetBytes(data);
            using var hmac = new HMACSHA256(keyBytes);
            var hash = hmac.ComputeHash(dataBytes);
            return BitConverter.ToString(hash).Replace("-", string.Empty).ToLowerInvariant();
        }
    }
}


