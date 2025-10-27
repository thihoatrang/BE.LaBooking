using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Users.Infrastructure.Models.Dtos;

namespace Users.Services.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserWithLawyerProfileController : ControllerBase
    {
        private readonly UserWithLawyerProfileService _service;

        public UserWithLawyerProfileController(UserWithLawyerProfileService service)
        {
            _service = service;
        }

        [HttpGet]
        [SwaggerOperation(
        Summary = "Xem full thông tin của tất cả tài khoản (có bio của luật sư)")]
        public async Task<ActionResult<ResponseDto<IEnumerable<UserWithLawyerProfileDTO>>>> GetAll()
        {
            var response = new ResponseDto<IEnumerable<UserWithLawyerProfileDTO>>();
            try
            {
                var result = await _service.GetAllUsersWithLawyerProfileAsync();
                response.Result = result;
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
                return StatusCode(500, response);
            }
            return Ok(response);
        }

        // GET: api/UserWithLawyerProfile/{userId}
        [HttpGet("{userId}")]
        [SwaggerOperation(
        Summary = "Xem full thông tin tài khoản theo id (có bio của luật sư)")]
        public async Task<ActionResult<ResponseDto<UserWithLawyerProfileDTO>>> GetByUserId(int userId)
        {
            var response = new ResponseDto<UserWithLawyerProfileDTO>();
            try
            {
                var result = await _service.GetUsersWithLawyerProfileByIdAsync(userId);
                if (result == null)
                {
                    response.IsSuccess = false;
                    response.Message = "User not found";
                    return NotFound(response);
                }
                response.Result = result;
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
                return StatusCode(500, response);
            }
            return Ok(response);
        }

        [HttpGet("only-lawyers")]
        [SwaggerOperation(
        Summary = "Xem full thông tin tất cả luật sư (dùng để show luật sư trang homepage)")]
        public async Task<ActionResult<ResponseDto<IEnumerable<UserWithLawyerProfileDTO>>>> GetUsersWithLawyerProfileOnly()
        {
            var response = new ResponseDto<IEnumerable<UserWithLawyerProfileDTO>>();
            try
            {

                var result = await _service.GetUsersWithLawyerProfileOnlyAsync();
                response.Result = result;
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
                return StatusCode(500, response);
            }
            return Ok(response);
        }

        [HttpGet("user-including-inactiveStatus")]
        [SwaggerOperation(
        Summary = "Xem full thông tin tài khoản (dùng cho Admin quản lý tất cả các tài khoản)")]
        public async Task<ActionResult<ResponseDto<IEnumerable<UserWithLawyerProfileDTO>>>> GetAllIncludingInactive()
        {
            var response = new ResponseDto<IEnumerable<UserWithLawyerProfileDTO>>();
            try
            {
                var result = await _service.GetAllUsersWithLawyerProfileIncludingInactiveAsync();
                response.Result = result;
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
                return StatusCode(500, response);
            }
            return Ok(response);
        }

        [HttpPut("{userId}")]
        [SwaggerOperation(
        Summary = "Sửa thông tin tài khoản (profile)")]
        public async Task<ActionResult<ResponseDto<bool>>> UpdateUserWithLawyerProfile(int userId, [FromBody] UpdateUserWithLawyerProfileDTO dto)
        {
            var response = await _service.UpdateUserWithLawyerProfileAsync(userId, dto);
            if (!response.IsSuccess)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }
    }
}