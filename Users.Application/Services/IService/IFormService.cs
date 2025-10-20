using System.Collections.Generic;
using System.Threading.Tasks;
using Users.Infrastructure.Models;
using Users.Infrastructure.Models.Dtos;

namespace Users.Application.Services.IService
{
    public interface IFormService
    {
        Task<IEnumerable<Form>> GetAllAsync();
        Task<Form?> GetByIdAsync(int id);
        Task<Form> CreateAsync(FormDTO dto);
        Task<Form?> UpdateAsync(int id, FormDTO dto);
        Task<bool> DeleteAsync(int id);
    }
} 