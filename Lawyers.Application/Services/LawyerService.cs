using AutoMapper;
using Lawyers.Infrastructure.Data;
using Lawyers.Infrastructure.Models;
using Lawyers.Infrastructure.Models.Dtos;
using Lawyers.Infrastructure.Repository;
using Lawyer.Application.Services.IService;
using Microsoft.EntityFrameworkCore;

namespace Lawyer.Application.Services
{
    public class LawyerService : ILawyerService
    {
        private readonly ILawyerProfileRepository _profileRepository;
        private readonly ILawyerPracticeAreaRepository _lawyerPracticeAreaRepository;
        private readonly IMapper _mapper;

        public LawyerService(ILawyerProfileRepository profileRepository, ILawyerPracticeAreaRepository lawyerPracticeAreaRepository, IMapper mapper)
        {
            _profileRepository = profileRepository;
            _lawyerPracticeAreaRepository = lawyerPracticeAreaRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<LawyerProfileDTO>> GetAllLawyersAsync()
        {
            var lawyers = await _profileRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<LawyerProfileDTO>>(lawyers);
        }

        public async Task<LawyerProfileDTO?> GetLawyerByIdAsync(int id)
        {
            var lawyer = await _profileRepository.GetByIdWithPracticeAreasAsync(id);
            return lawyer == null ? null : _mapper.Map<LawyerProfileDTO>(lawyer);
        }

        public async Task<LawyerProfileDTO> CreateLawyerAsync(LawyerProfileDTO profileDto)
        {
            var lawyerEntity = _mapper.Map<LawyerProfile>(profileDto);
            await _profileRepository.AddAsync(lawyerEntity);
            return _mapper.Map<LawyerProfileDTO>(lawyerEntity);
        }

        public async Task<bool> UpdateLawyerAsync(int id, LawyerProfileDTO profileDto)
        {
            var existing = await _profileRepository.GetByIdAsync(id);
            if (existing == null) return false;
            _mapper.Map(profileDto, existing);
            await _profileRepository.UpdateAsync(existing);
            return true;
        }

        public async Task<LawyerProfileDTO?> GetLawyerByUserIdAsync(int userId)
        {
            var lawyer = await _profileRepository.GetByUserIdAsync(userId);
            return lawyer == null ? null : _mapper.Map<LawyerProfileDTO>(lawyer);
        }

        public async Task<bool> DeleteLawyerAsync(int id)
        {
            var existing = await _profileRepository.GetByIdAsync(id);
            if (existing == null) return false;
            await _profileRepository.DeleteAsync(id);
            return true;
        }
        
        public async Task<LawyerProfileDTO?> UpdateLawyerProfileAsync(int id, UpdateLawyerDTO dto)
        {
            var lawyer = await _profileRepository.GetByIdAsync(id);
            if (lawyer == null) return null;
            
            // Cập nhật các trường từ DTO
            lawyer.Bio = dto.Bio;
            lawyer.LicenseNum = dto.LicenseNum;
            lawyer.ExpYears = dto.ExpYears;
            lawyer.Description = dto.Description;
            lawyer.Rating = dto.Rating;
            lawyer.PricePerHour = dto.PricePerHour;
            lawyer.Img = dto.Img;
            lawyer.DayOfWeek = dto.DayOfWeek;
            lawyer.WorkTime = dto.WorkTime;
            lawyer.UpdatedAt = DateTime.UtcNow;
            
            // Xóa các practice areas cũ và thêm mới
            await _lawyerPracticeAreaRepository.DeleteByLawyerIdAsync(id);
            
            foreach (var practiceAreaId in dto.PracticeAreaIds)
            {
                await _lawyerPracticeAreaRepository.AddAsync(new LawyerPracticeArea
                {
                    LawyerId = id,
                    PracticeAreaId = practiceAreaId
                });
            }
            
            await _profileRepository.UpdateAsync(lawyer);
            return _mapper.Map<LawyerProfileDTO>(await _profileRepository.GetByIdWithPracticeAreasAsync(id));
        }
    }
}
