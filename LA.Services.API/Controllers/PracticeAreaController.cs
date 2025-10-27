using AutoMapper;
using Lawyers.Application.Services.IService;
using Lawyers.Infrastructure.Models.Dtos;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace LA.Services.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PracticeAreaController : ControllerBase
    {
        private readonly IPracticeAreaService _practiceAreaService;
        private readonly IMapper _mapper;

        public PracticeAreaController(IPracticeAreaService practiceAreaService, IMapper mapper)
        {
            _practiceAreaService = practiceAreaService;
            _mapper = mapper;
        }

        [HttpGet]
        [SwaggerOperation(
        Summary = "Xem tất lĩnh vực")]
        public async Task<ActionResult<ResponseDto<IEnumerable<PracticeAreaDTO>>>> GetAll()
        {
            var response = new ResponseDto<IEnumerable<PracticeAreaDTO>>();

            try
            {
                var practiceAreas = await _practiceAreaService.GetAllPracticeAreasAsync();
                response.Result = practiceAreas;
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

        [HttpGet("{id}")]
        [SwaggerOperation(
        Summary = "Tìm lĩnh vực bằng id")]
        public async Task<ActionResult<ResponseDto<PracticeAreaDTO>>> GetById(int id)
        {
            var response = new ResponseDto<PracticeAreaDTO>();

            try
            {
                var practiceArea = await _practiceAreaService.GetPracticeAreaByIdAsync(id);
                if (practiceArea == null)
                {
                    response.IsSuccess = false;
                    response.Message = "Practice area not found";
                    return NotFound(response);
                }

                response.Result = practiceArea;
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

        [HttpGet("code/{code}")]
        [SwaggerOperation(
        Summary = "Tìm lĩnh vực theo mã")]
        public async Task<ActionResult<ResponseDto<PracticeAreaDTO>>> GetByCode(string code)
        {
            var response = new ResponseDto<PracticeAreaDTO>();

            try
            {
                var practiceArea = await _practiceAreaService.GetPracticeAreaByCodeAsync(code);
                if (practiceArea == null)
                {
                    response.IsSuccess = false;
                    response.Message = "Practice area not found";
                    return NotFound(response);
                }

                response.Result = practiceArea;
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

        [HttpPost]
        [SwaggerOperation(
        Summary = "Tạo lĩnh vực")]
        public async Task<ActionResult<ResponseDto<PracticeAreaDTO>>> Create([FromBody] PracticeAreaCreateDTO dto)
        {
            var response = new ResponseDto<PracticeAreaDTO>();
            PracticeAreaDTO? practiceArea = null;

            try
            {
                practiceArea = await _practiceAreaService.CreatePracticeAreaAsync(dto);
                response.Result = practiceArea;
                response.IsSuccess = true;
                response.Message = "Practice area created successfully";
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
                return StatusCode(500, response);
            }

            return CreatedAtAction(nameof(GetById), new { id = practiceArea?.Id }, response);
        }

        [HttpPut("{id}")]
        [SwaggerOperation(
        Summary = "Sửa lĩnh vực bằng id")]
        public async Task<ActionResult<ResponseDto<PracticeAreaDTO>>> Update(int id, [FromBody] PracticeAreaUpdateDTO dto)
        {
            var response = new ResponseDto<PracticeAreaDTO>();

            try
            {
                var success = await _practiceAreaService.UpdatePracticeAreaAsync(id, dto);
                if (!success)
                {
                    response.IsSuccess = false;
                    response.Message = "Practice area not found";
                    return NotFound(response);
                }

                var updatedPracticeArea = await _practiceAreaService.GetPracticeAreaByIdAsync(id);
                response.Result = updatedPracticeArea;
                response.IsSuccess = true;
                response.Message = "Practice area updated successfully";
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
                return StatusCode(500, response);
            }

            return Ok(response);
        }

        [HttpDelete("{id}")]
        [SwaggerOperation(
        Summary = "Xóa lĩnh vực bằng id")]
        public async Task<ActionResult<ResponseDto<object>>> Delete(int id)
        {
            var response = new ResponseDto<object>();

            try
            {
                var success = await _practiceAreaService.DeletePracticeAreaAsync(id);
                if (!success)
                {
                    response.IsSuccess = false;
                    response.Message = "Practice area not found";
                    return NotFound(response);
                }

                response.IsSuccess = true;
                response.Message = "Practice area deleted successfully";
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
                return StatusCode(500, response);
            }

            return Ok(response);
        }
    }
}
