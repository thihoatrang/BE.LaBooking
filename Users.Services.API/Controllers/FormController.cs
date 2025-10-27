using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Collections.Generic;
using System.Threading.Tasks;
using Users.Application.Services.IService;
using Users.Infrastructure.Models;
using Users.Infrastructure.Models.Dtos;

namespace Users.Services.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FormController : ControllerBase
    {
        private readonly IFormService _formService;
        public FormController(IFormService formService)
        {
            _formService = formService;
        }

        [HttpGet]
        [SwaggerOperation(
        Summary = "Xem tất cả văn bản luật")]
        public async Task<ActionResult<IEnumerable<Form>>> GetAll()
        {
            return Ok(await _formService.GetAllAsync());
        }

        [HttpGet("{id}")]
        [SwaggerOperation(
        Summary = "Xem tất cả văn bản luật qua Id")]
        public async Task<ActionResult<Form>> GetById(int id)
        {
            var form = await _formService.GetByIdAsync(id);
            if (form == null) return NotFound();
            return Ok(form);
        }

        [HttpPost]
        [SwaggerOperation(
        Summary = "Tạo văn bản luật")]
        public async Task<ActionResult<Form>> Create([FromBody] FormDTO dto)
        {
            var form = await _formService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = form.Id }, form);
        }

        [HttpPut("{id}")]
        [SwaggerOperation(
        Summary = "Sửa văn bản luật")]
        public async Task<ActionResult<Form>> Update(int id, [FromBody] FormDTO dto)
        {
            var form = await _formService.UpdateAsync(id, dto);
            if (form == null) return NotFound();
            return Ok(form);
        }

        [HttpDelete("{id}")]
        [SwaggerOperation(
        Summary = "Xóa văn bản luật")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _formService.DeleteAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }
    }
} 