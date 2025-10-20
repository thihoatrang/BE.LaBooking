using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Users.Infrastructure.Models.Dtos;
using Users.Application.Services.IService;

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