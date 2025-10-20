using Microsoft.AspNetCore.Mvc;
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