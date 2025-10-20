using Lawyers.Infrastructure.Models.Dtos;
using Lawyer.Application.Services.IService;
using Microsoft.AspNetCore.Mvc;

namespace LA.Services.API.Controllers
{
    [Route("api/lawyers/{lawyerId}/workslots")]
    [ApiController]
    public class WorkSlotAPIController : ControllerBase
    {
        private readonly IWorkSlotService _workSlotService;

        public WorkSlotAPIController(IWorkSlotService workSlotService)
        {
            _workSlotService = workSlotService;
        }

        [HttpGet]
        public async Task<IActionResult> GetWorkSlotsByLawyerId(int lawyerId)
        {
            ResponseDto<IEnumerable<WorkSlotDto>> _response = new();
            try
            {
                IEnumerable<WorkSlotDto> workSlots = await _workSlotService.GetWorkSlotsByLawyerIdAsync(lawyerId);
                _response.Result = workSlots;
                _response.IsSuccess = true;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return BadRequest(_response);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetWorkSlotById(int id)
        {
            ResponseDto<WorkSlotDto> _response = new();
            try
            {
                WorkSlotDto workSlot = await _workSlotService.GetWorkSlotByIdAsync(id);
                if (workSlot == null)
                {
                    _response.IsSuccess = false;
                    _response.Message = "WorkSlot not found.";
                    return NotFound(_response);
                }
                _response.Result = workSlot;
                _response.IsSuccess = true;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return BadRequest(_response);
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateWorkSlot(int lawyerId, [FromBody] CreateWorkSlotDto createWorkSlotDto)
        {
            ResponseDto<WorkSlotDto> _response = new();
            try
            {
                WorkSlotDto workSlot = await _workSlotService.CreateWorkSlotAsync(lawyerId, createWorkSlotDto);
                _response.Result = workSlot;
                _response.IsSuccess = true;
                return CreatedAtAction(nameof(GetWorkSlotById), new { lawyerId = workSlot.LawyerId, id = workSlot.Id }, _response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return BadRequest(_response);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateWorkSlot(int lawyerId, int id, [FromBody] UpdateWorkSlotDtoNoId updateWorkSlotDto)
        {
            ResponseDto<WorkSlotDto> _response = new();
            try
            {
                WorkSlotDto workSlot = await _workSlotService.UpdateWorkSlotAsync(lawyerId, updateWorkSlotDto, id);
                _response.Result = workSlot;
                _response.IsSuccess = true;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return BadRequest(_response);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteWorkSlot(int id)
        {
            ResponseDto<string> _response = new(); // Use string as the generic type for deletion response
            try
            {
                bool isDeleted = await _workSlotService.DeleteWorkSlotAsync(id);
                if (!isDeleted)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Failed to delete WorkSlot.";
                    return BadRequest(_response);
                }
                _response.IsSuccess = true;
                _response.Message = "WorkSlot deleted successfully.";
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return BadRequest(_response);
            }
        }

        [HttpPut("deactivate")]
        public async Task<IActionResult> DeactivateWorkSlot([FromBody] DeactivateWorkSlotDto dto)
        {
            var result = await _workSlotService.DeactivateWorkSlotAsync(dto);
            if (!result)
            {
                return NotFound(new { Message = "WorkSlot not found or already inactive." });
            }
            return Ok(new { Message = "WorkSlot deactivated successfully." });
        }

        [HttpPut("activate")]
        public async Task<IActionResult> ActivateWorkSlot([FromBody] ActivateWorkSlotDto dto)
        {
            var result = await _workSlotService.ActivateWorkSlotAsync(dto);
            if (!result)
            {
                return NotFound(new { Message = "WorkSlot not found or already active." });
            }
            return Ok(new { Message = "WorkSlot activated successfully." });
        }

        [HttpGet("/api/workslots")]
        public async Task<IActionResult> GetAllWorkSlots([FromQuery] bool includeInactive = false)
        {
            var workSlots = await _workSlotService.GetAllWorkSlotsAsync(includeInactive);
            return Ok(workSlots);
        }
    }
} 