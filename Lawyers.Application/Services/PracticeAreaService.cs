using AutoMapper;
using Lawyers.Infrastructure.Models;
using Lawyers.Infrastructure.Models.Dtos;
using Lawyers.Infrastructure.Repository;
using Lawyers.Application.Services.IService;

namespace Lawyers.Application.Services
{
    public class PracticeAreaService : IPracticeAreaService
    {
        private readonly IPracticeAreaRepository _practiceAreaRepository;
        private readonly IMapper _mapper;

        public PracticeAreaService(IPracticeAreaRepository practiceAreaRepository, IMapper mapper)
        {
            _practiceAreaRepository = practiceAreaRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<PracticeAreaDTO>> GetAllPracticeAreasAsync()
        {
            var practiceAreas = await _practiceAreaRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<PracticeAreaDTO>>(practiceAreas);
        }

        public async Task<PracticeAreaDTO?> GetPracticeAreaByIdAsync(int id)
        {
            var practiceArea = await _practiceAreaRepository.GetByIdAsync(id);
            return practiceArea == null ? null : _mapper.Map<PracticeAreaDTO>(practiceArea);
        }

        public async Task<PracticeAreaDTO?> GetPracticeAreaByCodeAsync(string code)
        {
            var practiceArea = await _practiceAreaRepository.GetByCodeAsync(code);
            return practiceArea == null ? null : _mapper.Map<PracticeAreaDTO>(practiceArea);
        }

        public async Task<PracticeAreaDTO> CreatePracticeAreaAsync(PracticeAreaCreateDTO dto)
        {
            var practiceArea = _mapper.Map<PracticeArea>(dto);
            await _practiceAreaRepository.AddAsync(practiceArea);
            return _mapper.Map<PracticeAreaDTO>(practiceArea);
        }

        public async Task<bool> UpdatePracticeAreaAsync(int id, PracticeAreaUpdateDTO dto)
        {
            var existing = await _practiceAreaRepository.GetByIdAsync(id);
            if (existing == null) return false;
            
            _mapper.Map(dto, existing);
            await _practiceAreaRepository.UpdateAsync(existing);
            return true;
        }

        public async Task<bool> DeletePracticeAreaAsync(int id)
        {
            var existing = await _practiceAreaRepository.GetByIdAsync(id);
            if (existing == null) return false;
            
            await _practiceAreaRepository.DeleteAsync(id);
            return true;
        }
    }
}
