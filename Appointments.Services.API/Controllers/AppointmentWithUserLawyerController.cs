using Appointments.Application.Services;
using Appointments.Infrastructure.Models;
using Appointments.Infrastructure.Models.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Appointments.Services.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AppointmentWithUserLawyerController : ControllerBase
    {
        private readonly AppointmentWithUserLawyerService _service;

        public AppointmentWithUserLawyerController(AppointmentWithUserLawyerService service)
        {
            _service = service;
        }

        [HttpGet("GetAllAppointment")]
        [SwaggerOperation(
        Summary = "Xem tất cả lịch hẹn")]
        public async Task<ActionResult<ResponseDto<IEnumerable<AppointmentWithUserLawyerDTO>>>> GetAll()
        {
            var response = new ResponseDto<IEnumerable<AppointmentWithUserLawyerDTO>>();
            try
            {
                var result = await _service.GetAllAppointmentsWithUserLawyerAsync();
                response.Result = result;
                response.DisplayMessage = "Lấy danh sách lịch hẹn thành công";
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.DisplayMessage = "Đã xảy ra lỗi";
                response.ErrorMessages = new List<string> { ex.Message };
                return StatusCode(500, response);
            }
            return Ok(response);
        }

        [HttpGet("GetAppointmentById/{id}")]
        [SwaggerOperation(
        Summary = "Tìm lịch hẹn bằng id")]
        public async Task<ActionResult<ResponseDto<AppointmentWithUserLawyerDTO>>> GetById(int id)
        {
            var response = new ResponseDto<AppointmentWithUserLawyerDTO>();
            try
            {
                var result = await _service.GetAppointmentWithUserLawyerByIdAsync(id);
                if (result == null)
                {
                    response.IsSuccess = false;
                    response.DisplayMessage = "Không tìm thấy lịch hẹn";
                    return NotFound(response);
                }
                response.Result = result;
                response.DisplayMessage = "Lấy chi tiết lịch hẹn thành công";
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.DisplayMessage = "Đã xảy ra lỗi";
                response.ErrorMessages = new List<string> { ex.Message };
                return StatusCode(500, response);
            }
            return Ok(response);
        }
        //Update
        [HttpPut("UpdateAppointment/{id}")]
        [SwaggerOperation(
        Summary = "Chỉnh sửa lịch hẹn (không dùng)")]
        public async Task<ActionResult<ResponseDto<AppointmentWithUserLawyerDTO>>> Update(int id, [FromBody] Appointment updatedAppointment)
        {
            var response = new ResponseDto<AppointmentWithUserLawyerDTO>();
            try
            {
                var result = await _service.UpdateAppointmentAsync(id, updatedAppointment);
                if (result == null)
                {
                    response.IsSuccess = false;
                    response.DisplayMessage = "Không tìm thấy lịch hẹn để cập nhật";
                    return NotFound(response);
                }
                response.Result = result;
                response.DisplayMessage = "Cập nhật lịch hẹn thành công";
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.DisplayMessage = "Đã xảy ra lỗi";
                response.ErrorMessages = new List<string> { ex.Message };
                return StatusCode(500, response);
            }
            return Ok(response);
        }

      

        // Khôi phục
        [HttpPut("restore/{id}")]
        [SwaggerOperation(
        Summary = "Khôi phục lịch hẹn")]
        public async Task<ActionResult<ResponseDto<bool>>> Restore(int id)
        {
            var response = new ResponseDto<bool>();
            try
            {
                var result = await _service.RestoreAppointmentAsync(id);
                response.Result = result;
                response.DisplayMessage = result ? "Khôi phục lịch hẹn thành công" : "Không tìm thấy lịch hẹn để khôi phục";
                if (!result)
                    return NotFound(response);
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.DisplayMessage = "Đã xảy ra lỗi khi khôi phục";
                response.ErrorMessages = new List<string> { ex.Message };
                return StatusCode(500, response);
            }
            return Ok(response);
        }

       
        // Lấy danh sách đã xóa mềm
        [HttpGet("GetDeletedAppointment")]
        [SwaggerOperation(
        Summary = "Xem lịch hẹn bị xóa")]
        public async Task<ActionResult<ResponseDto<IEnumerable<AppointmentWithUserLawyerDTO>>>> GetDeleted()
        {
            var response = new ResponseDto<IEnumerable<AppointmentWithUserLawyerDTO>>();
            try
            {
                var result = await _service.GetDeletedAppointmentsWithUserLawyerAsync();
                response.Result = result;
                response.DisplayMessage = "Lấy danh sách lịch hẹn đã xóa thành công";
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.DisplayMessage = "Đã xảy ra lỗi";
                response.ErrorMessages = new List<string> { ex.Message };
                return StatusCode(500, response);
            }
            return Ok(response);
        }

        [HttpGet("by-lawyer/{lawyerId}")]
        [SwaggerOperation(
        Summary = "Xem quản lí lịch hẹn (của luật sư)")]
        public async Task<ActionResult<IEnumerable<AppointmentWithUserLawyerDTO>>> GetByLawyerId(int lawyerId)
        {
            var result = await _service.GetAppointmentsByLawyerIdAsync(lawyerId);
            return Ok(result);
        }

        [HttpGet("by-user/{userId}")]
        [SwaggerOperation(
        Summary = "Xem quản lí lịch hẹn (của khách)")]
        public async Task<ActionResult<IEnumerable<AppointmentWithUserLawyerDTO>>> GetByUserId(int userId)
        {
            var result = await _service.GetAppointmentsByUserIdAsync(userId);
            return Ok(result);
        }


    }
}
    
