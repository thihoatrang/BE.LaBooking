using AutoMapper;
using Lawyers.Application.Services.IService;
using Lawyers.Infrastructure.Models.Dtos;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace LA.Services.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceController : ControllerBase
    {
        private readonly IServiceService _serviceService;
        private readonly IMapper _mapper;

        public ServiceController(IServiceService serviceService, IMapper mapper)
        {
            _serviceService = serviceService;
            _mapper = mapper;
        }

        [HttpGet]
        [SwaggerOperation(
        Summary = "Xem tất cả dịch vụ")]
        public async Task<ActionResult<ResponseDto<IEnumerable<ServiceDTO>>>> GetAll()
        {
            var response = new ResponseDto<IEnumerable<ServiceDTO>>();

            try
            {
                var services = await _serviceService.GetAllServicesAsync();
                response.Result = services;
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
        Summary = "Tạo dịch vụ")]
        public async Task<ActionResult<ResponseDto<ServiceDTO>>> GetById(int id)
        {
            var response = new ResponseDto<ServiceDTO>();

            try
            {
                var service = await _serviceService.GetServiceByIdAsync(id);
                if (service == null)
                {
                    response.IsSuccess = false;
                    response.Message = "Service not found";
                    return NotFound(response);
                }

                response.Result = service;
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

        [HttpGet("practice-area/{practiceAreaId}")]
        [SwaggerOperation(
        Summary = "Tìm dịch vụ bằng id lĩnh vực")]
        public async Task<ActionResult<ResponseDto<IEnumerable<ServiceDTO>>>> GetByPracticeAreaId(int practiceAreaId)
        {
            var response = new ResponseDto<IEnumerable<ServiceDTO>>();

            try
            {
                var services = await _serviceService.GetServicesByPracticeAreaIdAsync(practiceAreaId);
                response.Result = services;
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
        Summary = "Tìm dịch vụ bằng mã lĩnh vực")]
        public async Task<ActionResult<ResponseDto<ServiceDTO>>> GetByCode(string code)
        {
            var response = new ResponseDto<ServiceDTO>();

            try
            {
                var service = await _serviceService.GetServiceByCodeAsync(code);
                if (service == null)
                {
                    response.IsSuccess = false;
                    response.Message = "Service not found";
                    return NotFound(response);
                }

                response.Result = service;
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
        Summary = "Tạo dịch vụ")]
        public async Task<ActionResult<ResponseDto<ServiceDTO>>> Create([FromBody] ServiceCreateDTO dto)
        {
            var response = new ResponseDto<ServiceDTO>();
            ServiceDTO? service = null;

            try
            {
                service = await _serviceService.CreateServiceAsync(dto);
                response.Result = service;
                response.IsSuccess = true;
                response.Message = "Service created successfully";
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
                return StatusCode(500, response);
            }

            return CreatedAtAction(nameof(GetById), new { id = service?.Id }, response);
        }

        [HttpPut("{id}")]
        [SwaggerOperation(
        Summary = "Sửa dịch vụ")]
        public async Task<ActionResult<ResponseDto<ServiceDTO>>> Update(int id, [FromBody] ServiceUpdateDTO dto)
        {
            var response = new ResponseDto<ServiceDTO>();

            try
            {
                var success = await _serviceService.UpdateServiceAsync(id, dto);
                if (!success)
                {
                    response.IsSuccess = false;
                    response.Message = "Service not found";
                    return NotFound(response);
                }

                var updatedService = await _serviceService.GetServiceByIdAsync(id);
                response.Result = updatedService;
                response.IsSuccess = true;
                response.Message = "Service updated successfully";
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
        Summary = "Xóa dịch vụ")]
        public async Task<ActionResult<ResponseDto<object>>> Delete(int id)
        {
            var response = new ResponseDto<object>();

            try
            {
                var success = await _serviceService.DeleteServiceAsync(id);
                if (!success)
                {
                    response.IsSuccess = false;
                    response.Message = "Service not found";
                    return NotFound(response);
                }

                response.IsSuccess = true;
                response.Message = "Service deleted successfully";
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
