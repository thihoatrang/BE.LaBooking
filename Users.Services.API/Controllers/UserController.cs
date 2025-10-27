using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Users.Application.Services.IService;
using Users.Infrastructure.Models.Dtos;

namespace Users.Services.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }
        [HttpGet]
        [SwaggerOperation(
        Summary = "Xem tất cả tài khoản")]
        public async Task<ActionResult<ResponseDto<IEnumerable<UserDTO>>>> GetAllUsers([FromQuery] bool includeInactive = false)
        {
            var response = await _userService.GetAllUsersAsync(includeInactive);
            if (response.IsSuccess)
            {
                return Ok(response);
            }
            return BadRequest(response);
        }

        [HttpGet("{id}")]
        [SwaggerOperation(
        Summary = "Xem tài khoản qua id")]
        public async Task<ActionResult<ResponseDto<UserDTO>>> GetUserById(int id)
        {
            var response = await _userService.GetUserByIdAsync(id);
            if (response.IsSuccess)
            {
                return Ok(response);
            }
            return NotFound(response);
        }

        [HttpPost]
        [SwaggerOperation(
        Summary = "Thêm tài khoản")]
        public async Task<ActionResult<ResponseDto<UserDTO>>> CreateUser([FromBody] UserDTO userDto)
        {
            var response = await _userService.CreateUserAsync(userDto);
            if (response.IsSuccess)
            {
                return CreatedAtAction(nameof(GetUserById), new { id = response.Result.Id }, response);
            }
            return BadRequest(response);
        }

        [HttpPut("{id}")]
        [SwaggerOperation(
        Summary = "Sửa thông tin tài khoản")]
        public async Task<ActionResult<ResponseDto<bool>>> UpdateUser(int id, [FromBody] UserDTO userDto)
        {
            var response = await _userService.UpdateUserAsync(id, userDto);
            if (response.IsSuccess)
            {
                return Ok(response);
            }
            return NotFound(response);
        }

        [HttpDelete("soft/{id}")]
        [SwaggerOperation(
        Summary = "Xóa mềm tài khoản")]
        public async Task<ActionResult<ResponseDto<bool>>> SoftDeleteUser(int id)
        {
            var response = await _userService.SoftDeleteUserAsync(id);
            if (response.IsSuccess)
            {
                return Ok(response);
            }
            return NotFound(response);
        }

        [HttpDelete("hard/{id}")]
        [SwaggerOperation(
        Summary = "Xoá cứng tài khoản")]
        public async Task<ActionResult<ResponseDto<bool>>> HardDeleteUser(int id)
        {
            var response = await _userService.HardDeleteUserAsync(id);
            if (response.IsSuccess)
            {
                return Ok(response);
            }
            return NotFound(response);
        }

        [HttpPost("restore/{id}")]
        [SwaggerOperation(
        Summary = "Khôi phục tài khoản (khi bị Admin khóa)")]
        public async Task<ActionResult<ResponseDto<bool>>> RestoreUser(int id)
        {
            var response = await _userService.RestoreUserAsync(id);
            if (response.IsSuccess)
            {
                return Ok(response);
            }
            return NotFound(response);
        }
    }
} 