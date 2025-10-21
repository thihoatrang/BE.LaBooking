using AutoMapper;
using Lawyers.Infrastructure.Models;
using Lawyers.Infrastructure.Models.Dtos;
using Lawyers.Infrastructure.Repository;
using Lawyers.Application.Services.IService;

namespace Lawyers.Application.Services
{
    public class ServiceService : IServiceService
    {
        private readonly IServiceRepository _serviceRepository;
        private readonly IMapper _mapper;

        public ServiceService(IServiceRepository serviceRepository, IMapper mapper)
        {
            _serviceRepository = serviceRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ServiceDTO>> GetAllServicesAsync()
        {
            var services = await _serviceRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<ServiceDTO>>(services);
        }

        public async Task<ServiceDTO?> GetServiceByIdAsync(int id)
        {
            var service = await _serviceRepository.GetByIdAsync(id);
            return service == null ? null : _mapper.Map<ServiceDTO>(service);
        }

        public async Task<IEnumerable<ServiceDTO>> GetServicesByPracticeAreaIdAsync(int practiceAreaId)
        {
            var services = await _serviceRepository.GetByPracticeAreaIdAsync(practiceAreaId);
            return _mapper.Map<IEnumerable<ServiceDTO>>(services);
        }

        public async Task<ServiceDTO?> GetServiceByCodeAsync(string code)
        {
            var service = await _serviceRepository.GetByCodeAsync(code);
            return service == null ? null : _mapper.Map<ServiceDTO>(service);
        }

        public async Task<ServiceDTO> CreateServiceAsync(ServiceCreateDTO dto)
        {
            var service = _mapper.Map<Service>(dto);
            await _serviceRepository.AddAsync(service);
            return _mapper.Map<ServiceDTO>(service);
        }

        public async Task<bool> UpdateServiceAsync(int id, ServiceUpdateDTO dto)
        {
            var existing = await _serviceRepository.GetByIdAsync(id);
            if (existing == null) return false;
            
            _mapper.Map(dto, existing);
            await _serviceRepository.UpdateAsync(existing);
            return true;
        }

        public async Task<bool> DeleteServiceAsync(int id)
        {
            var existing = await _serviceRepository.GetByIdAsync(id);
            if (existing == null) return false;
            
            await _serviceRepository.DeleteAsync(id);
            return true;
        }
    }
}
