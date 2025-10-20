using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Users.Infrastructure.Models;
using Users.Infrastructure.Models.Dtos;
using Users.Application.Services.IService;

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
        public async Task<ActionResult<IEnumerable<Form>>> GetAll()
        {
            return Ok(await _formService.GetAllAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Form>> GetById(int id)
        {
            var form = await _formService.GetByIdAsync(id);
            if (form == null) return NotFound();
            return Ok(form);
        }

        [HttpPost]
        public async Task<ActionResult<Form>> Create([FromBody] FormDTO dto)
        {
            var form = await _formService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = form.Id }, form);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Form>> Update(int id, [FromBody] FormDTO dto)
        {
            var form = await _formService.UpdateAsync(id, dto);
            if (form == null) return NotFound();
            return Ok(form);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _formService.DeleteAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }
    }
} 