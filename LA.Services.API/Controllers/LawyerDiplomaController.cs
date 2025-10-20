using Microsoft.AspNetCore.Mvc;
using Lawyer.Application.Services.IService;
using Lawyers.Infrastructure.Models.Dtos;

namespace LA.Services.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LawyerDiplomaController : ControllerBase
    {
        private readonly ILawyerDiplomaService _diplomaService;

        public LawyerDiplomaController(ILawyerDiplomaService diplomaService)
        {
            _diplomaService = diplomaService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllDiplomas([FromQuery] bool includeDeleted = false)
        {
            var response = await _diplomaService.GetAllDiplomasAsync(includeDeleted);
            if (!response.IsSuccess)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDiplomaById(int id)
        {
            var response = await _diplomaService.GetDiplomaByIdAsync(id);
            if (!response.IsSuccess)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpGet("lawyer/{lawyerId}")]
        public async Task<IActionResult> GetDiplomasByLawyerId(int lawyerId, [FromQuery] bool includeDeleted = false)
        {
            var response = await _diplomaService.GetDiplomasByLawyerIdAsync(lawyerId, includeDeleted);
            if (!response.IsSuccess)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpPost("lawyer/{lawyerId}")]
        public async Task<IActionResult> CreateDiploma(int lawyerId, [FromBody] LawyerDiplomaCreateDto diplomaDto)
        {
            var response = await _diplomaService.CreateDiplomaAsync(lawyerId, diplomaDto);
            if (!response.IsSuccess)
            {
                return BadRequest(response);
            }
            return CreatedAtAction(nameof(GetDiplomaById), new { id = response.Result.Id }, response);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDiploma(int id, [FromBody] LawyerDiplomaUpdateDto diplomaDto)
        {
            var response = await _diplomaService.UpdateDiplomaAsync(id, diplomaDto);
            if (!response.IsSuccess)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDiploma(int id)
        {
            var response = await _diplomaService.DeleteDiplomaAsync(id);
            if (!response.IsSuccess)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpDelete("hard/{id}")]
        public async Task<IActionResult> DeleteDiplomaHard(int id)
        {
            var response = await _diplomaService.DeleteDiplomaHardAsync(id);
            if (!response.IsSuccess)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }
    }
} 