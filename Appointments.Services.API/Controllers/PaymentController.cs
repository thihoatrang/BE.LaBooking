using Appointments.Infrastructure.Models.Dtos;
using Appointments.Application.Services.IService;
using Appointments.Application.Services;
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
        private readonly IPaymentCalculationService _paymentCalculationService;
        private readonly ITransactionService _transactionService;

        public PaymentsController(
            IEnumerable<IPaymentProvider> providers, 
            Infrastructure.Repository.IPaymentRepository paymentRepo, 
            IConfiguration configuration,
            IPaymentCalculationService paymentCalculationService,
            ITransactionService transactionService)
        {
            _providers = providers;
            _paymentRepo = paymentRepo;
            _configuration = configuration;
            _paymentCalculationService = paymentCalculationService;
            _transactionService = transactionService;
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

        [HttpPost("create-url-for-appointment")]
        public async Task<ActionResult<string>> CreateUrlForAppointment([FromBody] CreatePaymentForAppointmentRequestDto request, CancellationToken ct)
        {
            try
            {
                // Calculate payment amount based on lawyer's PricePerHour
                var amount = await _paymentCalculationService.CalculatePaymentAmountAsync(request.LawyerId, request.DurationHours);
                
                // Generate unique order ID if not provided
                var orderId = string.IsNullOrEmpty(request.OrderId) ? Guid.NewGuid().ToString() : request.OrderId;
                
                var paymentRequest = new CreatePaymentRequestDto
                {
                    Vendor = request.Vendor,
                    OrderId = orderId,
                    Amount = amount,
                    OrderInfo = request.OrderInfo ?? $"Thanh toan cho ma GD: {orderId}",
                    ReturnUrl = request.ReturnUrl ?? _configuration["Payments:VnPay:ReturnUrl"]
                    // IpnUrl = request.IpnUrl ?? _configuration["Payments:VnPay:IpnUrl"] // Comment out for local development
                };

                var provider = _providers.FirstOrDefault(p => string.Equals(p.Vendor, request.Vendor, StringComparison.OrdinalIgnoreCase));
                if (provider == null) return BadRequest(new { message = "Unsupported vendor" });
                
                var resp = await provider.CreateAsync(paymentRequest, ct);
                
                // Save initial payment record
                await _paymentRepo.UpsertAsync(new Infrastructure.Models.Payment
                {
                    OrderId = orderId,
                    Vendor = provider.Vendor,
                    Amount = amount,
                    Status = "pending"
                }, ct);
                
                return Ok(resp.PaymentUrl);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    Error = "Failed to create payment URL", 
                    Details = ex.Message 
                });
            }
        }

        // VNPAY return and IPN come with query params
        [HttpGet("return")]
        public async Task<ActionResult<PaymentStatusDto>> Return()
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
            
            // Update payment status and create transaction records
            await _transactionService.UpdatePaymentStatusAsync(status.OrderId, status.Status, status.TransactionId, status.Message);
            
            return Ok(status);
        }

        [HttpGet("ipn")]
        public async Task<ActionResult<string>> Ipn()
        {
            var vendor = Request.Query["vendor"].ToString();
            var provider = _providers.FirstOrDefault(p => string.Equals(p.Vendor, vendor, StringComparison.OrdinalIgnoreCase));
            if (provider == null) return BadRequest("Unsupported vendor");

            var dict = Request.Query.ToDictionary(k => k.Key, v => v.Value.ToString());
            var signatureKey = "vnp_SecureHash"; // Only VnPay is supported now
            var verified = provider.VerifySignature(dict, signatureKey);
            var status = provider.ParseCallback(dict);
            
            if (!verified)
            {
                return Ok("Invalid signature");
            }

            // Update payment status and create transaction records
            await _transactionService.UpdatePaymentStatusAsync(status.OrderId, status.Status, status.TransactionId, status.Message);

            return Ok("OK"); // VNPAY expects a 200 OK plain text
        }

        [HttpGet("status/{orderId}")]
        public ActionResult<PaymentStatusDto> Status(string orderId)
        {
            var p = _paymentRepo.GetByOrderIdAsync(orderId).GetAwaiter().GetResult();
            if (p == null) return NotFound();
            return Ok(new PaymentStatusDto { OrderId = p.OrderId, Vendor = p.Vendor, Status = p.Status, Message = p.Message, TransactionId = p.TransactionId });
        }

        [HttpGet("appointment/{orderId}")]
        public async Task<ActionResult<object>> GetAppointmentByOrderId(string orderId)
        {
            try
            {
                var payment = await _paymentRepo.GetByOrderIdAsync(orderId);
                if (payment == null)
                {
                    return NotFound(new { message = "Payment not found" });
                }

                // In a real implementation, you would fetch the appointment details
                // For now, we'll return payment information
                return Ok(new
                {
                    OrderId = payment.OrderId,
                    Amount = payment.Amount,
                    Status = payment.Status,
                    TransactionId = payment.TransactionId,
                    CreatedAt = payment.CreatedAt,
                    Message = payment.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    Error = "Failed to get appointment information", 
                    Details = ex.Message 
                });
            }
        }

    }
}


