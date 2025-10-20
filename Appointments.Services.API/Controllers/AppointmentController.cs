using Appointments.Infrastructure.Models;
using Appointments.Infrastructure.Models.Dtos;
using Appointments.Infrastructure.Models.Saga;
using Appointments.Application.Services;
using Appointments.Application.Services.IService;
using Appointments.Application.Services.Saga;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Appointments.Services.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;
        private readonly IAppointmentSagaService _sagaService;

        public AppointmentController(IAppointmentService appointmentService, IAppointmentSagaService sagaService)
        {
            _appointmentService = appointmentService;
            _sagaService = sagaService;
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
        public async Task<IActionResult> UpdateConfirmedStatus(int id)
        {
            var result = await _appointmentService.UpdateConfirmedStatusAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }

        [HttpPut("{id}/cancel")]
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

        [HttpPut("{id}/complete")]
        public async Task<IActionResult> UpdateCompletedStatus(int id)
        {
            var result = await _appointmentService.UpdateCompletedStatusAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStatus(int id)
        {
            var result = await _appointmentService.DeleteStatusAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }

        [HttpPut("UpdateAppointment/{id}")]
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

        [HttpGet("{id}/saga-state")]
        public async Task<IActionResult> GetSagaState(int id)
        {
            try
            {
                var sagaState = await _sagaService.GetSagaStateAsync(id);
                if (sagaState == null) return NotFound();
                
                return Ok(sagaState);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    Error = "Failed to get saga state", 
                    Details = ex.Message 
                });
            }
        }

    }
}
