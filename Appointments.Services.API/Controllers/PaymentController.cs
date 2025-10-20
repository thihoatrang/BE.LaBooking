using Appointments.Infrastructure.Models.Dtos;
using Appointments.Application.Services.IService;
using Microsoft.AspNetCore.Mvc;

namespace Appointments.Services.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly IEnumerable<IPaymentProvider> _providers;
        private readonly Infrastructure.Repository.IPaymentRepository _paymentRepo;
        private readonly IConfiguration _configuration;

        public PaymentsController(IEnumerable<IPaymentProvider> providers, Infrastructure.Repository.IPaymentRepository paymentRepo, IConfiguration configuration)
        {
            _providers = providers;
            _paymentRepo = paymentRepo;
            _configuration = configuration;
        }

        [HttpPost("create")]
        public async Task<ActionResult<CreatePaymentResponseDto>> Create([FromBody] CreatePaymentRequestDto request, CancellationToken ct)
        {
            var provider = _providers.FirstOrDefault(p => string.Equals(p.Vendor, request.Vendor, StringComparison.OrdinalIgnoreCase));
            if (provider == null) return BadRequest(new { message = "Unsupported vendor" });
            var resp = await provider.CreateAsync(request, ct);
            // save initial payment record
            await _paymentRepo.UpsertAsync(new Infrastructure.Models.Payment
            {
                OrderId = request.OrderId,
                Vendor = provider.Vendor,
                Amount = request.Amount,
                Status = "pending"
            }, ct);
            return Ok(resp);
        }

        // VNPAY return and IPN come with query params
        [HttpGet("return")]
        public ActionResult<PaymentStatusDto> Return()
        {
            var vendor = Request.Query["vendor"].ToString();
            var provider = _providers.FirstOrDefault(p => string.Equals(p.Vendor, vendor, StringComparison.OrdinalIgnoreCase));
            if (provider == null) return BadRequest(new { message = "Unsupported vendor" });

            var dict = Request.Query.ToDictionary(k => k.Key, v => v.Value.ToString());
            var signatureKey = vendor.Equals("vnpay", StringComparison.OrdinalIgnoreCase) ? "vnp_SecureHash" : "signature";
            var verified = provider.VerifySignature(dict, signatureKey);
            var status = provider.ParseCallback(dict);
            if (!verified)
            {
                status.Status = "failed";
                status.Message = "Signature verification failed";
            }
            // persist status
            _ = _paymentRepo.UpsertAsync(new Infrastructure.Models.Payment
            {
                OrderId = status.OrderId,
                Vendor = status.Vendor,
                Status = status.Status,
                TransactionId = status.TransactionId,
                Message = status.Message
            });
            return Ok(status);
        }

        [HttpGet("ipn")]
        public async Task<ActionResult<string>> Ipn()
        {
            var vendor = Request.Query["vendor"].ToString();
            var provider = _providers.FirstOrDefault(p => string.Equals(p.Vendor, vendor, StringComparison.OrdinalIgnoreCase));
            if (provider == null) return BadRequest("Unsupported vendor");

            var dict = Request.Query.ToDictionary(k => k.Key, v => v.Value.ToString());
            var signatureKey = vendor.Equals("vnpay", StringComparison.OrdinalIgnoreCase) ? "vnp_SecureHash" : "signature";
            var verified = provider.VerifySignature(dict, signatureKey);
            var status = provider.ParseCallback(dict);
            // TODO: update order status in DB here
            if (!verified)
            {
                if (vendor.Equals("vnpay", StringComparison.OrdinalIgnoreCase))
                    return Ok("Invalid signature");
                return BadRequest("invalid signature");
            }

            await _paymentRepo.UpsertAsync(new Infrastructure.Models.Payment
            {
                OrderId = status.OrderId,
                Vendor = status.Vendor,
                Status = status.Status,
                TransactionId = status.TransactionId,
                Message = status.Message
            });

            if (vendor.Equals("vnpay", StringComparison.OrdinalIgnoreCase))
                return Ok("OK"); // VNPAY expects a 200 OK plain text

            return Ok("0"); // MoMo expects 200; custom body not strictly required here
        }

        [HttpGet("status/{orderId}")]
        public ActionResult<PaymentStatusDto> Status(string orderId)
        {
            var p = _paymentRepo.GetByOrderIdAsync(orderId).GetAwaiter().GetResult();
            if (p == null) return NotFound();
            return Ok(new PaymentStatusDto { OrderId = p.OrderId, Vendor = p.Vendor, Status = p.Status, Message = p.Message, TransactionId = p.TransactionId });
        }

        [HttpGet("qr")]
        public ActionResult GetQr([FromQuery] string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return BadRequest();
            // Simple QR via Google Chart (dev only). For prod, use a library.
            var qrUrl = $"https://chart.googleapis.com/chart?cht=qr&chs=300x300&chl={Uri.EscapeDataString(url)}";
            return Redirect(qrUrl);
        }

        [HttpPost("ipn/momo")]
        public async Task<ActionResult<object>> MoMoIpn([FromBody] MoMoIpnDto body)
        {
            var provider = _providers.FirstOrDefault(p => p.Vendor == "momo");
            if (provider == null) return BadRequest();

            var accessKey = _configuration["Payments:MoMo:AccessKey"] ?? string.Empty;

            var dict = new Dictionary<string, string>
            {
                ["accessKey"] = accessKey,
                ["amount"] = body.amount.ToString(),
                ["extraData"] = body.extraData,
                ["message"] = body.message,
                ["orderId"] = body.orderId,
                ["orderInfo"] = body.orderInfo,
                ["orderType"] = body.orderType,
                ["partnerCode"] = body.partnerCode,
                ["payType"] = body.payType,
                ["requestId"] = body.requestId,
                ["responseTime"] = body.responseTime.ToString(),
                ["resultCode"] = body.resultCode.ToString(),
                ["transId"] = body.transId.ToString()
            };

            var valid = provider.VerifySignature(dict, "signature");
            var status = provider.ParseCallback(dict);
            if (!valid)
            {
                return Ok(new { resultCode = 1, message = "signature invalid" });
            }

            await _paymentRepo.UpsertAsync(new Infrastructure.Models.Payment
            {
                OrderId = status.OrderId,
                Vendor = status.Vendor,
                Status = status.Status,
                TransactionId = status.TransactionId,
                Message = status.Message,
                Amount = body.amount
            });

            return Ok(new { resultCode = 0, message = "ok" });
        }
    }
}


