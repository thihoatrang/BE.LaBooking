using AutoMapper;
using Lawyers.Infrastructure.Models.Dtos;
using Lawyers.Infrastructure.Models.Saga;
using Lawyer.Application.Services;
using Lawyer.Application.Services.IService;
using Lawyer.Application.Services.Saga;
using Microsoft.AspNetCore.Mvc;

namespace LA.Services.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LawyerController : ControllerBase
    {
        private readonly ILawyerService _service;
        private readonly ILawyerSagaService _sagaService;
        private readonly IMapper _mapper;

        public LawyerController(ILawyerService service, ILawyerSagaService sagaService, IMapper mapper)
        {
            _service = service;
            _sagaService = sagaService;
            _mapper = mapper;
        }

        [HttpGet("GetAllLawyerProfile")]
        public async Task<ActionResult<ResponseDto<IEnumerable<LawyerProfileDTO>>>> GetAll()
        {
            var response = new ResponseDto<IEnumerable<LawyerProfileDTO>>();

            try
            {
                var lawyers = await _service.GetAllLawyersAsync();
                response.Result = lawyers;
                response.IsSuccess = true;
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
                return StatusCode(500, response);
            }

            return Ok(response);
        }

        // GET: api/lawyer/GetProfileById/5
        [HttpGet("GetProfileById/{id}")]
        public async Task<ActionResult<ResponseDto<LawyerProfileDTO>>> GetById(int id)
        {
            var response = new ResponseDto<LawyerProfileDTO>();

            try
            {
                var profile = await _service.GetLawyerByIdAsync(id);
                if (profile == null)
                {
                    response.IsSuccess = false;
                    response.Message = $"No profile found with id = {id}";
                    return NotFound(response);
                }

                response.Result = profile;
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
                return StatusCode(500, response);
            }

            return Ok(response);
        }

        // POST: api/lawyer/CreateLawyerProfile
        //[HttpPost("CreateLawyerProfile")]
        //public async Task<ActionResult<ResponseDto<LawyerProfileDTO>>> CreateLawyerProfile([FromBody] LawyerProfileDTO dto)
        //{
        //    var response = new ResponseDto<LawyerProfileDTO>();

        //    try
        //    {
        //        var created = await _service.CreateLawyerAsync(dto);
        //        response.Result = created;
        //        return CreatedAtAction(nameof(GetById), new { id = created.Id }, response);
        //    }
        //    catch (Exception ex)
        //    {
        //        response.IsSuccess = false;
        //        response.Message = ex.Message;
        //        return StatusCode(500, response);
        //    }
        //}

        // PUT: api/lawyer/UpdateLawyerProfile/5
        [HttpPut("UpdateLawyerProfile/{id}")]
        public async Task<IActionResult> UpdateLawyerProfile(int id, [FromBody] LawyerProfileDTO dto)
        {
            try
            {
                // Use Saga Pattern for lawyer profile update
                var sagaData = await _sagaService.StartLawyerUpdateSagaAsync(id, dto);
                
                return Ok(new ResponseDto<bool>
                {
                    IsSuccess = true,
                    Result = true,
                    Message = "Lawyer profile updated successfully using Saga Pattern"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDto<bool>
                {
                    IsSuccess = false,
                    Message = $"Failed to update lawyer profile: {ex.Message}"
                });
            }
        }

        // DELETE: api/lawyer/DeleteLawyerProfile/5
        //[HttpDelete("DeleteLawyerProfile/{id}")]
        //public async Task<IActionResult> DeleteLawyerProfile(int id)
        //{
        //    var response = new ResponseDto<bool>();

        //    try
        //    {
        //        var success = await _service.DeleteLawyerAsync(id);
        //        if (!success)
        //        {
        //            response.IsSuccess = false;
        //            response.Message = $"No lawyer profile found with id = {id}";
        //            return NotFound(response);
        //        }

        //        response.Result = true;
        //        return Ok(response);
        //    }
        //    catch (Exception ex)
        //    {
        //        response.IsSuccess = false;
        //        response.Message = ex.Message;
        //        return StatusCode(500, response);
        //    }
        //}
        [HttpGet("GetProfileByUserId/{userId}")]
        public async Task<ActionResult<ResponseDto<LawyerProfileDTO>>> GetProfileByUserId(int userId)
        {
            var response = new ResponseDto<LawyerProfileDTO>();
            try
            {
                var profile = await _service.GetLawyerByUserIdAsync(userId);
                if (profile == null)
                {
                    response.IsSuccess = false;
                    response.Message = $"No profile found with userId = {userId}";
                    return NotFound(response);
                }
                response.Result = profile;
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
                return StatusCode(500, response);
            }
            return Ok(response);
        }
        //UPDATE
        [HttpPut("UpdateLawyerByLaywerId/{id}")]
        public async Task<IActionResult> UpdateLawyer(int id, [FromBody] UpdateLawyerDTO dto)
        {
            try
            {
                var updated = await _service.UpdateLawyerProfileAsync(id, dto);
                if (updated == null) return NotFound();
                
                return Ok(new { 
                    Lawyer = updated,
                    Message = "Lawyer profile updated successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    Error = "Failed to update lawyer profile", 
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
