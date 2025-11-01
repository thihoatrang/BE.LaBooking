using Appointments.Application.Services;
using Appointments.Application.Services.IService;
using Appointments.Application.Services.Saga;
using Appointments.Infrastructure.Models;
using Appointments.Infrastructure.Models.Dtos;
using Appointments.Infrastructure.Models.Saga;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Appointments.Services.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;
        private readonly IAppointmentSagaService _sagaService;
        private readonly Infrastructure.Repository.IAppointmentRepository _appointmentRepository;

        public AppointmentController(
            IAppointmentService appointmentService, 
            IAppointmentSagaService sagaService,
            Infrastructure.Repository.IAppointmentRepository appointmentRepository)
        {
            _appointmentService = appointmentService;
            _sagaService = sagaService;
            _appointmentRepository = appointmentRepository;
        }
        //[HttpPost]
        //public async Task<ActionResult<ResponseDto<Appointment>>> CreateAppointment([FromBody] CreateAppointmentDTO dto)
        //{
        //    var response = new ResponseDto<Appointment>();

        //    try
        //    {
        //        var result = await _appointmentService.CreateAppointmentAsync(dto);
        //        response.Result = result;
        //    }
        //    catch (Exception ex)
        //    {
        //        response.IsSuccess = false;
        //        response.DisplayMessage = ex.Message;
        //        return StatusCode(500, response);
        //    }

        //    return Ok(response);
        //}
        [HttpPost("CREATE")]
        [SwaggerOperation(
        Summary = "Tạo lịch hẹn")]
        public async Task<IActionResult> Create([FromBody] CreateAppointmentDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                // Use Saga Pattern for transaction management
                var sagaData = await _sagaService.StartSagaAsync(dto);
                return Ok(new { 
                    AppointmentId = sagaData.AppointmentId, 
                    State = sagaData.State.ToString(),
                    Message = "Appointment created successfully using Saga Pattern"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    Error = "Failed to create appointment", 
                    Details = ex.Message 
                });
            }
        }

        [HttpPut("{id}/confirm")]
        [SwaggerOperation(
        Summary = "Luật sư confirm lịch hẹn")]
        public async Task<IActionResult> UpdateConfirmedStatus(int id)
        {
            var result = await _appointmentService.UpdateConfirmedStatusAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }

        [HttpPut("{id}/cancel")]
        [SwaggerOperation(
        Summary = "Luật sư hoặc khách hủy lịch hẹn")]
        public async Task<IActionResult> UpdateCancelledStatus(int id)
        {
            try
            {
                var result = await _appointmentService.UpdateCancelledStatusAsync(id);
                if (!result) return NotFound();
                
                // Compensate saga if needed
                await _sagaService.CompensateSagaAsync(id, "Appointment cancelled by user");
                
                return Ok(new { 
                    Message = "Appointment cancelled successfully",
                    SagaCompensated = true
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    Error = "Failed to cancel appointment", 
                    Details = ex.Message 
                });
            }
        }

        [HttpPut("{id}/cancel-payment-pending")]
        [SwaggerOperation(
        Summary = "Hủy lịch hẹn khi đang ở trạng thái PaymentPending (thanh toán failed)")]
        public async Task<IActionResult> CancelPaymentPendingAppointment(int id)
        {
            try
            {
                var appointment = await _appointmentRepository.GetByIdAsync(id);
                if (appointment == null)
                {
                    return NotFound(new { message = "Appointment not found" });
                }

                // Only allow cancellation if status is PaymentPending
                if (appointment.Status != (int)Appointments.Infrastructure.Models.Enums.AppointmentStatus.PaymentPending)
                {
                    return BadRequest(new { 
                        message = $"Appointment is not in PaymentPending state. Current status: {appointment.Status}",
                        currentStatus = appointment.Status
                    });
                }

                // Cancel appointment and compensate saga
                var result = await _appointmentService.UpdateCancelledStatusAsync(id);
                if (!result) return NotFound();
                
                // Compensate saga: reactivate work slot
                await _sagaService.CompensateSagaAsync(id, "Appointment cancelled - payment pending timeout");
                
                return Ok(new { 
                    Message = "Payment pending appointment cancelled successfully",
                    AppointmentId = id,
                    SagaCompensated = true
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    Error = "Failed to cancel payment pending appointment", 
                    Details = ex.Message 
                });
            }
        }

        [HttpPut("{id}/complete")]
        [SwaggerOperation(
        Summary = "Luật sư hoàn thành lịch hẹn")]
        public async Task<IActionResult> UpdateCompletedStatus(int id)
        {
            var result = await _appointmentService.UpdateCompletedStatusAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }

        [HttpDelete("{id}")]
        [SwaggerOperation(
        Summary = "Xóa lịch hẹn (không dùng)")]
        public async Task<IActionResult> DeleteStatus(int id)
        {
            var result = await _appointmentService.DeleteStatusAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }

        [HttpPut("UpdateAppointment/{id}")]
        [SwaggerOperation(
        Summary = "Chỉnh sửa lịch hẹn (không dùng)")]
        public async Task<IActionResult> UpdateAppointment(int id, [FromBody] UpdateAppointmentDTO dto)
        {
            try
            {
                var updated = await _appointmentService.UpdateAppointmentAsync(id, dto);
                if (updated == null) return NotFound();
                
                return Ok(new { 
                    Appointment = updated,
                    Message = "Appointment updated successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    Error = "Failed to update appointment", 
                    Details = ex.Message 
                });
            }
        }

        [HttpGet("all")]
        [SwaggerOperation(
        Summary = "Lấy tất cả appointments với pagination và filtering")]
        public async Task<ActionResult<object>> GetAllAppointments(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 1000,
            [FromQuery] int? status = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] bool? isDel = false)
        {
            try
            {
                // Validate pagination
                if (page < 1) page = 1;
                if (pageSize < 1) pageSize = 10;
                if (pageSize > 1000) pageSize = 1000;

                var allAppointments = (await _appointmentRepository.GetAllAsync()).ToList();
                Console.WriteLine($"[AppointmentController] Retrieved {allAppointments.Count} appointments from repository");

                // Apply filters
                var query = allAppointments.AsQueryable();

                if (isDel.HasValue)
                {
                    query = query.Where(a => a.IsDel == isDel.Value);
                }
                else
                {
                    query = query.Where(a => !a.IsDel); // Default: only active appointments
                }

                if (status.HasValue)
                {
                    query = query.Where(a => a.Status == status.Value);
                }

                if (fromDate.HasValue && query.Any(a => a.CreateAt.HasValue))
                {
                    query = query.Where(a => a.CreateAt.HasValue && 
                        a.CreateAt.Value.ToDateTime(TimeOnly.MinValue) >= fromDate.Value);
                }

                if (toDate.HasValue && query.Any(a => a.CreateAt.HasValue))
                {
                    var endDate = toDate.Value.AddDays(1);
                    query = query.Where(a => a.CreateAt.HasValue && 
                        a.CreateAt.Value.ToDateTime(TimeOnly.MinValue) < endDate);
                }

                var totalCount = query.Count();
                Console.WriteLine($"[AppointmentController] After filters: {totalCount} appointments match criteria");

                // Apply pagination and ordering
                var appointments = query
                    .OrderByDescending(a => a.CreateAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(a => new
                    {
                        Id = a.Id,
                        UserId = a.UserId,
                        LawyerId = a.LawyerId,
                        ScheduledAt = a.ScheduledAt,
                        Slot = a.Slot,
                        Status = a.Status,
                        Services = a.Services,
                        Note = a.Note,
                        Spec = a.Spec,
                        CreateAt = a.CreateAt,
                        IsDel = a.IsDel
                    })
                    .ToList();

                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
                Console.WriteLine($"[AppointmentController] Returning {appointments.Count} appointments (page {page}/{totalPages})");
                return Ok(new
                {
                    Appointments = appointments?.Cast<object>().ToList() ?? new List<object>(),
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
                        IsDel = isDel ?? false
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Error = "Failed to get appointments",
                    Details = ex.Message
                });
            }
        }

        //[HttpGet("{id}/saga-state")]
        //public async Task<IActionResult> GetSagaState(int id)
        //{
        //    try
        //    {
        //        var sagaState = await _sagaService.GetSagaStateAsync(id);
        //        if (sagaState == null) return NotFound();
                
        //        return Ok(sagaState);
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { 
        //            Error = "Failed to get saga state", 
        //            Details = ex.Message 
        //        });
        //    }
        //}

    }
}
