using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Users.Infrastructure.Data;
using Users.Infrastructure.Models;
using Users.Infrastructure.Models.Dtos;
using Users.Infrastructure.Repository;
using Users.Application.Services.IService;

namespace Users.Application.Services
{
    public class FormService : IFormService
    {
        private readonly IFormRepository _formRepository;
        public FormService(IFormRepository formRepository)
        {
            _formRepository = formRepository;
        }

        public async Task<IEnumerable<Form>> GetAllAsync()
        {
            return await _formRepository.GetAllAsync();
        }

        public async Task<Form?> GetByIdAsync(int id)
        {
            return await _formRepository.GetByIdAsync(id);
        }

        public async Task<Form> CreateAsync(FormDTO dto)
        {
            var form = new Form
            {
                Name = dto.Name,
                Description = dto.Description,
                FilePath = dto.FilePath
            };
            await _formRepository.AddAsync(form);
            return form;
        }

        public async Task<Form?> UpdateAsync(int id, FormDTO dto)
        {
            var form = await _formRepository.GetByIdAsync(id);
            if (form == null) return null;
            form.Name = dto.Name;
            form.Description = dto.Description;
            form.FilePath = dto.FilePath;
            await _formRepository.UpdateAsync(form);
            return form;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var form = await _formRepository.GetByIdAsync(id);
            if (form == null) return false;
            await _formRepository.DeleteAsync(id);
            return true;
        }
    }
} 