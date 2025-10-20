using Lawyers.Infrastructure.Data;
using Lawyers.Infrastructure.Models;
using Lawyers.Infrastructure.Models.Dtos;
using Lawyers.Infrastructure.Repository;
using Lawyers.Infrastructure.Models.Saga;
using Lawyer.Application.Services.IService;
using Microsoft.Extensions.Logging;

namespace Lawyer.Application.Services.Saga
{
    public class LawyerSagaService : ILawyerSagaService
    {
        private readonly ILawyerProfileRepository _lawyerRepository;
        private readonly IWorkSlotRepository _workSlotRepository;
        private readonly ILogger<LawyerSagaService> _logger;
        private readonly Dictionary<int, LawyerSagaData> _sagaStates = new();

        public LawyerSagaService(
            ILawyerProfileRepository lawyerRepository,
            IWorkSlotRepository workSlotRepository,
            ILogger<LawyerSagaService> logger)
        {
            _lawyerRepository = lawyerRepository;
            _workSlotRepository = workSlotRepository;
            _logger = logger;
        }

        public async Task<LawyerSagaData> StartLawyerCreationSagaAsync(LawyerProfileDTO dto)
        {
            var sagaData = new LawyerSagaData
            {
                UserId = dto.UserId,
                Bio = dto.Bio,
                Spec = string.Join(",", dto.Spec ?? new List<string>()),
                LicenseNum = dto.LicenseNum,
                ExpYears = dto.ExpYears,
                Description = dto.Description,
                Rating = (decimal)dto.Rating,
                PricePerHour = (decimal)dto.PricePerHour,
                Img = dto.Img,
                DayOfWeek = dto.DayOfWeek,
                WorkTime = dto.WorkTime,
                State = LawyerSagaState.Started,
                CreatedAt = DateTime.UtcNow
            };

            try
            {
                // Step 1: Create lawyer profile
                var lawyer = new LawyerProfile
                {
                    UserId = dto.UserId,
                    Bio = dto.Bio,
                    Spec = dto.Spec ?? new List<string>(),
                    LicenseNum = dto.LicenseNum,
                    ExpYears = dto.ExpYears,
                    Description = dto.Description,
                    Rating = dto.Rating,
                    PricePerHour = dto.PricePerHour,
                    Img = dto.Img,
                    DayOfWeek = dto.DayOfWeek,
                    WorkTime = dto.WorkTime
                };

                await _lawyerRepository.AddAsync(lawyer);
                sagaData.LawyerId = lawyer.Id;
                _sagaStates[lawyer.Id] = sagaData;

                _logger.LogInformation($"Lawyer creation saga started for lawyer {lawyer.Id}");

                // Step 2: Create work slots
                await CreateWorkSlotsAsync(lawyer.Id);

                // Step 3: Complete saga
                await CompleteSagaAsync(lawyer.Id);

                return sagaData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lawyer creation saga failed for lawyer {sagaData.LawyerId}");
                sagaData.State = LawyerSagaState.Failed;
                sagaData.ErrorMessage = ex.Message;
                
                if (sagaData.LawyerId > 0)
                {
                    await CompensateSagaAsync(sagaData.LawyerId, ex.Message);
                }
                
                throw;
            }
        }

        public async Task<LawyerSagaData> StartLawyerUpdateSagaAsync(int id, LawyerProfileDTO dto)
        {
            var sagaData = new LawyerSagaData
            {
                LawyerId = id,
                UserId = dto.UserId,
                Bio = dto.Bio,
                Spec = string.Join(",", dto.Spec ?? new List<string>()),
                LicenseNum = dto.LicenseNum,
                ExpYears = dto.ExpYears,
                Description = dto.Description,
                Rating = (decimal)dto.Rating,
                PricePerHour = (decimal)dto.PricePerHour,
                Img = dto.Img,
                DayOfWeek = dto.DayOfWeek,
                WorkTime = dto.WorkTime,
                State = LawyerSagaState.Started,
                CreatedAt = DateTime.UtcNow
            };

            try
            {
                // Step 1: Update lawyer profile
                var lawyer = await _lawyerRepository.GetByIdAsync(id);
                if (lawyer == null)
                    throw new InvalidOperationException($"Lawyer {id} not found");

                lawyer.Bio = dto.Bio;
                lawyer.Spec = dto.Spec;
                lawyer.LicenseNum = dto.LicenseNum;
                lawyer.ExpYears = dto.ExpYears;
                lawyer.Description = dto.Description;
                lawyer.Rating = dto.Rating;
                lawyer.PricePerHour = dto.PricePerHour;
                lawyer.Img = dto.Img;
                lawyer.DayOfWeek = dto.DayOfWeek;
                lawyer.WorkTime = dto.WorkTime;

                await _lawyerRepository.UpdateAsync(lawyer);
                _sagaStates[id] = sagaData;

                _logger.LogInformation($"Lawyer update saga started for lawyer {id}");

                // Step 2: Update work slots if needed
                await UpdateWorkSlotsAsync(id);

                // Step 3: Complete saga
                await CompleteSagaAsync(id);

                return sagaData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lawyer update saga failed for lawyer {id}");
                sagaData.State = LawyerSagaState.Failed;
                sagaData.ErrorMessage = ex.Message;
                
                await CompensateSagaAsync(id, ex.Message);
                throw;
            }
        }

        private async Task CreateWorkSlotsAsync(int lawyerId)
        {
            var sagaData = _sagaStates[lawyerId];
            var lawyer = await _lawyerRepository.GetByIdAsync(lawyerId);
            
            if (lawyer == null)
                throw new InvalidOperationException($"Lawyer {lawyerId} not found");

            // Parse day of week and create work slots
            var days = lawyer.DayOfWeek?.Split(',') ?? new string[0];
            var timeSlots = ParseWorkTime(lawyer.WorkTime);

            foreach (var day in days)
            {
                foreach (var timeSlot in timeSlots)
                {
                    var workSlot = new WorkSlot
                    {
                        LawyerId = lawyerId,
                        DayOfWeek = day.Trim(),
                        Slot = timeSlot,
                        IsActive = true
                    };

                    await _workSlotRepository.AddAsync(workSlot);
                    sagaData.WorkSlotIds.Add(workSlot.Id);
                }
            }

            sagaData.State = LawyerSagaState.WorkSlotsCreated;
            _logger.LogInformation($"Work slots created for lawyer {lawyerId}");
        }

        private async Task UpdateWorkSlotsAsync(int lawyerId)
        {
            var sagaData = _sagaStates[lawyerId];
            var lawyer = await _lawyerRepository.GetByIdAsync(lawyerId);
            
            if (lawyer == null)
                throw new InvalidOperationException($"Lawyer {lawyerId} not found");

            // Get existing work slots
            var existingSlots = await _workSlotRepository.GetByLawyerIdAsync(lawyerId);
            
            // Delete existing slots
            foreach (var slot in existingSlots)
            {
                await _workSlotRepository.DeleteAsync(slot.Id);
            }

            // Create new work slots
            await CreateWorkSlotsAsync(lawyerId);

            _logger.LogInformation($"Work slots updated for lawyer {lawyerId}");
        }

        private List<string> ParseWorkTime(string workTime)
        {
            // Simple parsing - can be enhanced based on your format
            if (string.IsNullOrEmpty(workTime))
                return new List<string> { "09:00-10:00", "10:00-11:00", "14:00-15:00", "15:00-16:00" };

            // Example: "08:00-12:00,13:00-17:00" -> ["08:00-09:00", "09:00-10:00", ...]
            var timeRanges = workTime.Split(',');
            var slots = new List<string>();

            foreach (var range in timeRanges)
            {
                var parts = range.Split('-');
                if (parts.Length == 2)
                {
                    var start = TimeSpan.Parse(parts[0].Trim());
                    var end = TimeSpan.Parse(parts[1].Trim());
                    
                    while (start < end)
                    {
                        var nextHour = start.Add(TimeSpan.FromHours(1));
                        if (nextHour <= end)
                        {
                            slots.Add($"{start:hh\\:mm}-{nextHour:hh\\:mm}");
                        }
                        start = nextHour;
                    }
                }
            }

            return slots.Any() ? slots : new List<string> { "09:00-10:00", "10:00-11:00", "14:00-15:00", "15:00-16:00" };
        }

        public async Task<bool> CompleteSagaAsync(int lawyerId)
        {
            if (!_sagaStates.ContainsKey(lawyerId))
                return false;

            var sagaData = _sagaStates[lawyerId];
            sagaData.State = LawyerSagaState.Completed;
            sagaData.CompletedAt = DateTime.UtcNow;
            
            _logger.LogInformation($"Lawyer saga completed for lawyer {lawyerId}");
            return true;
        }

        public async Task<bool> CompensateSagaAsync(int lawyerId, string reason)
        {
            if (!_sagaStates.ContainsKey(lawyerId))
                return false;

            var sagaData = _sagaStates[lawyerId];
            sagaData.State = LawyerSagaState.Compensating;
            
            try
            {
                // Compensate: Delete work slots
                if (sagaData.State == LawyerSagaState.WorkSlotsCreated)
                {
                    foreach (var slotId in sagaData.WorkSlotIds)
                    {
                        await _workSlotRepository.DeleteAsync(slotId);
                    }
                }

                // Compensate: Delete lawyer profile
                var lawyer = await _lawyerRepository.GetByIdAsync(lawyerId);
                if (lawyer != null)
                {
                    await _lawyerRepository.DeleteAsync(lawyerId);
                }

                sagaData.State = LawyerSagaState.Failed;
                sagaData.ErrorMessage = reason;
                
                _logger.LogInformation($"Lawyer saga compensated for lawyer {lawyerId}: {reason}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to compensate lawyer saga for lawyer {lawyerId}");
                return false;
            }
        }

        public async Task<LawyerSagaData?> GetSagaStateAsync(int lawyerId)
        {
            return _sagaStates.ContainsKey(lawyerId) ? _sagaStates[lawyerId] : null;
        }

        public async Task<bool> UpdateSagaStateAsync(int lawyerId, LawyerSagaState newState, string? errorMessage = null)
        {
            if (!_sagaStates.ContainsKey(lawyerId))
                return false;

            var sagaData = _sagaStates[lawyerId];
            sagaData.State = newState;
            if (!string.IsNullOrEmpty(errorMessage))
            {
                sagaData.ErrorMessage = errorMessage;
            }

            return true;
        }
    }
}
