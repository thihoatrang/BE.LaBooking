using AutoMapper;
using Lawyers.Infrastructure.Data;
using Lawyers.Infrastructure.Models;
using Lawyers.Infrastructure.Models.Dtos;
using Lawyers.Infrastructure.Repository;
using Lawyer.Application.Services.IService;
using Microsoft.EntityFrameworkCore;

namespace Lawyer.Application.Services
{
    public class WorkSlotService : IWorkSlotService
    {
        private readonly IWorkSlotRepository _workSlotRepository;
        private readonly IMapper _mapper;

        public WorkSlotService(IWorkSlotRepository workSlotRepository, IMapper mapper)
        {
            _workSlotRepository = workSlotRepository;
            _mapper = mapper;
        }

        public async Task<WorkSlotDto> CreateWorkSlotAsync(int lawyerId, CreateWorkSlotDto createWorkSlotDto)
        {
            WorkSlot workSlot = _mapper.Map<WorkSlot>(createWorkSlotDto);
            workSlot.LawyerId = lawyerId;
            await _workSlotRepository.AddAsync(workSlot);
            return _mapper.Map<WorkSlotDto>(workSlot);
        }

        public async Task<bool> DeleteWorkSlotAsync(int id)
        {
            try
            {
                WorkSlot workSlot = await _workSlotRepository.GetByIdAsync(id);
                if (workSlot == null)
                {
                    return false;
                }
                workSlot.IsActive = false;
                await _workSlotRepository.UpdateAsync(workSlot);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<IEnumerable<WorkSlotDto>> GetAllWorkSlotsAsync()
        {
            return await GetAllWorkSlotsAsync(false);
        }

        public async Task<IEnumerable<WorkSlotDto>> GetAllWorkSlotsAsync(bool includeInactive = false)
        {
            IEnumerable<WorkSlot> workSlots = await _workSlotRepository.GetAllAsync();
            if (!includeInactive)
            {
                workSlots = workSlots.Where(ws => ws.IsActive);
            }
            return _mapper.Map<IEnumerable<WorkSlotDto>>(workSlots);
        }

        public async Task<WorkSlotDto> GetWorkSlotByIdAsync(int id)
        {
            WorkSlot workSlot = await _workSlotRepository.GetByIdAsync(id);
            if (workSlot == null || !workSlot.IsActive) return null;
            return _mapper.Map<WorkSlotDto>(workSlot);
        }

        public async Task<WorkSlotDto> UpdateWorkSlotAsync(int lawyerId, UpdateWorkSlotDtoNoId updateWorkSlotDto, int workSlotId)
        {
            var workSlot = await _workSlotRepository.GetByIdAsync(workSlotId);
            if (workSlot == null)
            {
                return null;
            }
            _mapper.Map(updateWorkSlotDto, workSlot);
            workSlot.LawyerId = lawyerId;
            await _workSlotRepository.UpdateAsync(workSlot);
            return _mapper.Map<WorkSlotDto>(workSlot);
        }

        public async Task<IEnumerable<WorkSlotDto>> GetWorkSlotsByLawyerIdAsync(int lawyerId)
        {
            IEnumerable<WorkSlot> workSlots = (await _workSlotRepository.GetAllAsync()).Where(ws => ws.LawyerId == lawyerId && (ws.IsActive == true) || (ws.IsActive == false));
            return _mapper.Map<IEnumerable<WorkSlotDto>>(workSlots);
        }

        public async Task<bool> DeactivateWorkSlotAsync(DeactivateWorkSlotDto dto)
        {
            var workSlots = await _workSlotRepository.GetAllAsync();
            var workSlot = workSlots.FirstOrDefault(ws => ws.Slot == dto.Slot && ws.DayOfWeek == dto.DayOfWeek && ws.LawyerId == dto.LawyerId && ws.IsActive);
            if (workSlot == null)
            {
                return false;
            }
            workSlot.IsActive = false;
            await _workSlotRepository.UpdateAsync(workSlot);
            return true;
        }

        public async Task<bool> ActivateWorkSlotAsync(ActivateWorkSlotDto dto)
        {
            var workSlots = await _workSlotRepository.GetAllAsync();
            var workSlot = workSlots.FirstOrDefault(ws => ws.Slot == dto.Slot && ws.DayOfWeek == dto.DayOfWeek && ws.LawyerId == dto.LawyerId && !ws.IsActive);
            if (workSlot == null)
            {
                return false;
            }
            workSlot.IsActive = true;
            await _workSlotRepository.UpdateAsync(workSlot);
            return true;
        }
    }
} 