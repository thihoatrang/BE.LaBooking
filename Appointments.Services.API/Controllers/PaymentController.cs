using Appointments.Application.Services;
using Appointments.Application.Services.IService;
using Appointments.Infrastructure.Models.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;

namespace Appointments.Services.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly IEnumerable<IPaymentProvider> _providers;
        private readonly Infrastructure.Repository.IPaymentRepository _paymentRepo;
        private readonly Infrastructure.Repository.IAppointmentRepository _appointmentRepository;
        private readonly IConfiguration _configuration;
        private readonly IPaymentCalculationService _paymentCalculationService;
        private readonly ITransactionService _transactionService;
        private readonly ILogger<PaymentsController> _logger;

        public PaymentsController(
            IEnumerable<IPaymentProvider> providers, 
            Infrastructure.Repository.IPaymentRepository paymentRepo,
            Infrastructure.Repository.IAppointmentRepository appointmentRepository,
            IConfiguration configuration,
            IPaymentCalculationService paymentCalculationService,
            ITransactionService transactionService,
            ILogger<PaymentsController> logger)
        {
            _providers = providers;
            _paymentRepo = paymentRepo;
            _appointmentRepository = appointmentRepository;
            _configuration = configuration;
            _paymentCalculationService = paymentCalculationService;
            _transactionService = transactionService;
            _logger = logger;
        }

        [HttpPost("create")]
        [SwaggerOperation(
        Summary = "Không dùng")]
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
                Status = "pending",
                CreatedAt = DateTime.Now // Use local time (Vietnam UTC+7)
            }, ct);
            return Ok(resp);
        }

        [HttpPost("create-url-for-appointment")]
        [SwaggerOperation(
        Summary = "Thanh toán VNPay")]
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
                    Status = "pending",
                    AppointmentId = request.AppointmentId,
                    CreatedAt = DateTime.Now // Use local time (Vietnam UTC+7)
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
        [SwaggerOperation(
        Summary = "Trả lại bên frontend")]
        public async Task<ActionResult<PaymentStatusDto>> Return()
        {
            try
            {
                // VNPay doesn't send vendor in query, default to vnpay
                var vendor = Request.Query["vendor"].ToString();
                if (string.IsNullOrEmpty(vendor))
                {
                    vendor = "vnpay"; // Default to vnpay if not provided
                }
                
                var provider = _providers.FirstOrDefault(p => string.Equals(p.Vendor, vendor, StringComparison.OrdinalIgnoreCase));
                if (provider == null) return BadRequest(new { message = "Unsupported vendor" });

                // Get raw query string to preserve encoding
                var rawQueryString = Request.QueryString.Value ?? string.Empty;
                _logger.LogInformation($"Raw query string: {rawQueryString}");
                
                var dict = Request.Query.ToDictionary(k => k.Key, v => v.Value.ToString());
                
                _logger.LogInformation($"Payment callback received with {dict.Count} parameters");
                _logger.LogInformation($"Payment callback keys: {string.Join(", ", dict.Keys)}");
                
                var signatureKey = vendor.Equals("vnpay", StringComparison.OrdinalIgnoreCase) ? "vnp_SecureHash" : "signature";
                var verified = provider.VerifySignature(dict, signatureKey);
                var status = provider.ParseCallback(dict);
                
                if (!verified)
                {
                    _logger.LogWarning($"Signature verification failed for OrderId: {status.OrderId}");
                    _logger.LogWarning($"Available signature parameter: {dict.ContainsKey(signatureKey)}");
                    if (dict.ContainsKey(signatureKey))
                    {
                        _logger.LogWarning($"Signature value length: {dict[signatureKey]?.Length ?? 0}");
                    }
                    status.Status = "failed";
                    status.Message = "Signature verification failed";
                }
                else
                {
                    _logger.LogInformation($"Signature verified successfully for OrderId: {status.OrderId}");
                }
                
                _logger.LogInformation($"Payment return callback received - OrderId: {status.OrderId}, Status: {status.Status}, Verified: {verified}");
                
                // Update payment status and create transaction records
                var updateResult = await _transactionService.UpdatePaymentStatusAsync(status.OrderId, status.Status, status.TransactionId, status.Message);
                
                if (!updateResult)
                {
                    _logger.LogWarning($"Failed to update payment status for OrderId: {status.OrderId}");
                }
                else
                {
                    _logger.LogInformation($"Payment status updated successfully for OrderId: {status.OrderId}");
                }
                
                return Ok(status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment return callback");
                return StatusCode(500, new { message = "Error processing payment return", error = ex.Message });
            }
        }

        [HttpGet("ipn")]
        [SwaggerOperation(
        Summary = "Chưa dùng tới")]
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
        [SwaggerOperation(
        Summary = "Xem trạng thái thanh toán của lịch hẹn")]
        public ActionResult<PaymentStatusDto> Status(string orderId)
        {
            var p = _paymentRepo.GetByOrderIdAsync(orderId).GetAwaiter().GetResult();
            if (p == null) return NotFound();
            return Ok(new PaymentStatusDto { OrderId = p.OrderId, Vendor = p.Vendor, Status = p.Status, Message = p.Message, TransactionId = p.TransactionId });
        }

        [HttpGet("appointment/{orderId}")]
        [SwaggerOperation(
        Summary = "Xem payment theo OrderId (thanh toán)")]
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
                    Message = payment.Message,
                    AppointmentId = payment.AppointmentId
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

        [HttpGet("all")]
        [SwaggerOperation(
        Summary = "Lấy tất cả payments với pagination và filtering - dùng để quản lý tất cả payment trong hệ thống")]
        public async Task<ActionResult<object>> GetAllPayments(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? status = null,
            [FromQuery] string? vendor = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] bool includeAppointment = false,
            CancellationToken ct = default)
        {
            try
            {
                // Validate pagination parameters
                if (page < 1) page = 1;
                if (pageSize < 1) pageSize = 10;
                if (pageSize > 100) pageSize = 100; // Limit max page size

                var (payments, totalCount) = await _paymentRepo.GetAllAsync(
                    page: page,
                    pageSize: pageSize,
                    status: status,
                    vendor: vendor,
                    fromDate: fromDate,
                    toDate: toDate,
                    ct: ct);

                // Build payment list with optional appointment data
                var paymentList = new List<object>();
                
                foreach (var p in payments)
                {
                    var paymentDto = new
                    {
                        Id = p.Id,
                        OrderId = p.OrderId,
                        Vendor = p.Vendor,
                        Amount = p.Amount,
                        Status = p.Status,
                        TransactionId = p.TransactionId,
                        Message = p.Message,
                        AppointmentId = p.AppointmentId,
                        CreatedAt = p.CreatedAt,
                        UpdatedAt = p.UpdatedAt,
                        BankCode = p.BankCode,
                        BankTranNo = p.BankTranNo,
                        PayDate = p.PayDate,
                        OrderInfo = p.OrderInfo
                    };

                    // If includeAppointment is true, fetch appointment details
                    if (includeAppointment && p.AppointmentId.HasValue)
                    {
                        try
                        {
                            var appointment = await _appointmentRepository.GetByIdAsync(p.AppointmentId.Value);
                            if (appointment != null)
                            {
                                paymentList.Add(new
                                {
                                    paymentDto.Id,
                                    paymentDto.OrderId,
                                    paymentDto.Vendor,
                                    paymentDto.Amount,
                                    paymentDto.Status,
                                    paymentDto.TransactionId,
                                    paymentDto.Message,
                                    paymentDto.AppointmentId,
                                    paymentDto.CreatedAt,
                                    paymentDto.UpdatedAt,
                                    paymentDto.BankCode,
                                    paymentDto.BankTranNo,
                                    paymentDto.PayDate,
                                    paymentDto.OrderInfo,
                                    Appointment = new
                                    {
                                        Id = appointment.Id,
                                        UserId = appointment.UserId,
                                        LawyerId = appointment.LawyerId,
                                        ScheduledAt = appointment.ScheduledAt,
                                        Slot = appointment.Slot,
                                        Status = appointment.Status,
                                        Services = appointment.Services,
                                        Note = appointment.Note,
                                        Spec = appointment.Spec,
                                        CreateAt = appointment.CreateAt,
                                        IsDel = appointment.IsDel
                                    }
                                });
                            }
                            else
                            {
                                paymentList.Add(paymentDto);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, $"Failed to fetch appointment {p.AppointmentId} for payment {p.OrderId}");
                            paymentList.Add(paymentDto);
                        }
                    }
                    else
                    {
                        paymentList.Add(paymentDto);
                    }
                }

                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                return Ok(new
                {
                    Payments = paymentList,
                    Pagination = new
                    {
                        Page = page,
                        PageSize = pageSize,
                        TotalCount = totalCount,
                        TotalPages = totalPages,
                        HasNextPage = page < totalPages,
                        HasPreviousPage = page > 1
                    },
                    Filters = new
                    {
                        Status = status,
                        Vendor = vendor,
                        FromDate = fromDate,
                        ToDate = toDate,
                        IncludeAppointment = includeAppointment
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get all payments");
                return StatusCode(500, new
                {
                    Error = "Failed to get payments",
                    Details = ex.Message
                });
            }
        }

        [HttpGet("by-appointment/{appointmentId}")]
        [SwaggerOperation(
        Summary = "Xem payment theo AppointmentId - dùng để xem chi tiết lịch hẹn sau khi thanh toán")]
        public async Task<ActionResult<object>> GetPaymentByAppointmentId(int appointmentId)
        {
            try
            {
                var payment = await _paymentRepo.GetByAppointmentIdAsync(appointmentId);
                if (payment == null)
                {
                    return NotFound(new { message = "No payment found for this appointment" });
                }

                // Get appointment details
                var appointment = await _appointmentRepository.GetByIdAsync(appointmentId);
                if (appointment == null)
                {
                    return NotFound(new { message = "Appointment not found" });
                }

                return Ok(new
                {
                    Payment = new
                    {
                        OrderId = payment.OrderId,
                        Amount = payment.Amount,
                        Status = payment.Status,
                        TransactionId = payment.TransactionId,
                        CreatedAt = payment.CreatedAt,
                        UpdatedAt = payment.UpdatedAt,
                        Message = payment.Message,
                        Vendor = payment.Vendor,
                        AppointmentId = payment.AppointmentId
                    },
                    Appointment = new
                    {
                        Id = appointment.Id,
                        UserId = appointment.UserId,
                        LawyerId = appointment.LawyerId,
                        ScheduledAt = appointment.ScheduledAt,
                        Slot = appointment.Slot,
                        Status = appointment.Status,
                        Services = appointment.Services,
                        Note = appointment.Note,
                        Spec = appointment.Spec
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to get payment by appointmentId {appointmentId}");
                return StatusCode(500, new { 
                    Error = "Failed to get payment information", 
                    Details = ex.Message 
                });
            }
        }

        [HttpGet("by-user/{userId}")]
        [SwaggerOperation(
        Summary = "Lấy tất cả payments của một user - dùng để user xem lịch sử thanh toán của mình")]
        public async Task<ActionResult<object>> GetPaymentsByUserId(
            int userId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? status = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] bool includeAppointment = false,
            CancellationToken ct = default)
        {
            try
            {
                // Validate pagination parameters
                if (page < 1) page = 1;
                if (pageSize < 1) pageSize = 10;
                if (pageSize > 100) pageSize = 100; // Limit max page size

                var (payments, totalCount) = await _paymentRepo.GetByUserIdAsync(
                    userId: userId,
                    page: page,
                    pageSize: pageSize,
                    status: status,
                    fromDate: fromDate,
                    toDate: toDate,
                    ct: ct);

                // Build payment list with optional appointment data
                var paymentList = new List<object>();

                foreach (var p in payments)
                {
                    var paymentDto = new
                    {
                        Id = p.Id,
                        OrderId = p.OrderId,
                        Vendor = p.Vendor,
                        Amount = p.Amount,
                        Status = p.Status,
                        TransactionId = p.TransactionId,
                        Message = p.Message,
                        AppointmentId = p.AppointmentId,
                        CreatedAt = p.CreatedAt,
                        UpdatedAt = p.UpdatedAt,
                        BankCode = p.BankCode,
                        BankTranNo = p.BankTranNo,
                        PayDate = p.PayDate,
                        OrderInfo = p.OrderInfo
                    };

                    // If includeAppointment is true, fetch appointment details
                    if (includeAppointment && p.AppointmentId.HasValue)
                    {
                        try
                        {
                            var appointment = await _appointmentRepository.GetByIdAsync(p.AppointmentId.Value);
                            if (appointment != null)
                            {
                                paymentList.Add(new
                                {
                                    paymentDto.Id,
                                    paymentDto.OrderId,
                                    paymentDto.Vendor,
                                    paymentDto.Amount,
                                    paymentDto.Status,
                                    paymentDto.TransactionId,
                                    paymentDto.Message,
                                    paymentDto.AppointmentId,
                                    paymentDto.CreatedAt,
                                    paymentDto.UpdatedAt,
                                    paymentDto.BankCode,
                                    paymentDto.BankTranNo,
                                    paymentDto.PayDate,
                                    paymentDto.OrderInfo,
                                    Appointment = new
                                    {
                                        Id = appointment.Id,
                                        UserId = appointment.UserId,
                                        LawyerId = appointment.LawyerId,
                                        ScheduledAt = appointment.ScheduledAt,
                                        Slot = appointment.Slot,
                                        Status = appointment.Status,
                                        Services = appointment.Services,
                                        Note = appointment.Note,
                                        Spec = appointment.Spec,
                                        CreateAt = appointment.CreateAt,
                                        IsDel = appointment.IsDel
                                    }
                                });
                            }
                            else
                            {
                                paymentList.Add(paymentDto);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, $"Failed to fetch appointment {p.AppointmentId} for payment {p.OrderId}");
                            paymentList.Add(paymentDto);
                        }
                    }
                    else
                    {
                        paymentList.Add(paymentDto);
                    }
                }

                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                return Ok(new
                {
                    UserId = userId,
                    Payments = paymentList,
                    Pagination = new
                    {
                        Page = page,
                        PageSize = pageSize,
                        TotalCount = totalCount,
                        TotalPages = totalPages,
                        HasNextPage = page < totalPages,
                        HasPreviousPage = page > 1
                    },
                    Filters = new
                    {
                        Status = status,
                        FromDate = fromDate,
                        ToDate = toDate,
                        IncludeAppointment = includeAppointment
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to get payments for user {userId}");
                return StatusCode(500, new
                {
                    Error = "Failed to get payments",
                    Details = ex.Message
                });
            }
        }

        [HttpPost("retry-payment/{appointmentId}")]
        [SwaggerOperation(
        Summary = "Retry payment cho appointment - dùng khi payment failed và user muốn thanh toán lại")]
        public async Task<ActionResult<string>> RetryPayment(int appointmentId, [FromBody] CreatePaymentForAppointmentRequestDto? request = null, CancellationToken ct = default)
        {
            try
            {
                // Get appointment to verify it exists and is in PaymentPending state
                var appointment = await _appointmentRepository.GetByIdAsync(appointmentId);
                if (appointment == null)
                {
                    return NotFound(new { message = "Appointment not found" });
                }

                // Check if appointment is in PaymentPending state
                if (appointment.Status != (int)Appointments.Infrastructure.Models.Enums.AppointmentStatus.PaymentPending)
                {
                    return BadRequest(new { 
                        message = $"Appointment is not in PaymentPending state. Current status: {appointment.Status}",
                        allowedStatus = (int)Appointments.Infrastructure.Models.Enums.AppointmentStatus.PaymentPending
                    });
                }

                // Use provided request or get from appointment
                var paymentRequest = request ?? new CreatePaymentForAppointmentRequestDto
                {
                    Vendor = "vnpay",
                    LawyerId = appointment.LawyerId,
                    DurationHours = 1,
                    AppointmentId = appointmentId,
                    OrderInfo = $"Retry payment for appointment {appointmentId}",
                    ReturnUrl = _configuration["Payments:VnPay:ReturnUrl"]
                };

                // Ensure AppointmentId matches
                paymentRequest.AppointmentId = appointmentId;
                paymentRequest.LawyerId = appointment.LawyerId;

                // Calculate payment amount
                var amount = await _paymentCalculationService.CalculatePaymentAmountAsync(appointment.LawyerId, paymentRequest.DurationHours);
                
                // Generate new unique order ID for retry
                var guidShort = Guid.NewGuid().ToString("N").Substring(0, 8);
                var orderId = $"RETRY-{appointmentId}-{DateTime.Now:yyyyMMddHHmmss}-{guidShort}"; // Use local time
                
                var createPaymentRequest = new CreatePaymentRequestDto
                {
                    Vendor = paymentRequest.Vendor,
                    OrderId = orderId,
                    Amount = amount,
                    OrderInfo = paymentRequest.OrderInfo ?? $"Retry thanh toan cho appointment {appointmentId}",
                    ReturnUrl = paymentRequest.ReturnUrl ?? _configuration["Payments:VnPay:ReturnUrl"]
                };

                var provider = _providers.FirstOrDefault(p => string.Equals(p.Vendor, paymentRequest.Vendor, StringComparison.OrdinalIgnoreCase));
                if (provider == null) return BadRequest(new { message = "Unsupported vendor" });
                
                var resp = await provider.CreateAsync(createPaymentRequest, ct);
                
                // Save new payment record for retry
                await _paymentRepo.UpsertAsync(new Infrastructure.Models.Payment
                {
                    OrderId = orderId,
                    Vendor = provider.Vendor,
                    Amount = amount,
                    Status = "pending",
                    AppointmentId = appointmentId,
                    CreatedAt = DateTime.Now // Use local time (Vietnam UTC+7)
                }, ct);
                
                _logger.LogInformation($"Retry payment created for appointment {appointmentId}. New OrderId: {orderId}");
                
                return Ok(resp.PaymentUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to retry payment for appointment {appointmentId}");
                return StatusCode(500, new { 
                    Error = "Failed to create retry payment URL", 
                    Details = ex.Message 
                });
            }
        }

    }
}


