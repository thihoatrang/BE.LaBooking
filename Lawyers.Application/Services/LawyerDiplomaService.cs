using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Lawyers.Infrastructure.Data;
using Lawyers.Infrastructure.Models;
using Lawyers.Infrastructure.Models.Dtos;
using Lawyer.Application.Services.IService;
using Lawyers.Infrastructure.Repository;

namespace Lawyer.Application.Services
{
    public class LawyerDiplomaService : ILawyerDiplomaService
    {
        private readonly ILawyerDiplomaRepository _diplomaRepository;
        private readonly IMapper _mapper;

        public LawyerDiplomaService(ILawyerDiplomaRepository diplomaRepository, IMapper mapper)
        {
            _diplomaRepository = diplomaRepository;
            _mapper = mapper;
        }

        public async Task<ResponseDto<IEnumerable<LawyerDiplomaDTO>>> GetAllDiplomasAsync(bool includeDeleted = false)
        {
            var response = new ResponseDto<IEnumerable<LawyerDiplomaDTO>>();
            try
            {
                var diplomas = await _diplomaRepository.GetAllAsync();
                if (!includeDeleted)
                {
                    diplomas = diplomas.Where(d => !d.IsDeleted);
                }
                response.Result = _mapper.Map<IEnumerable<LawyerDiplomaDTO>>(diplomas);
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            return response;
        }

        public async Task<ResponseDto<LawyerDiplomaDTO>> GetDiplomaByIdAsync(int id)
        {
            var response = new ResponseDto<LawyerDiplomaDTO>();
            try
            {
                var diploma = await _diplomaRepository.GetByIdAsync(id);
                if (diploma == null)
                {
                    response.IsSuccess = false;
                    response.Message = $"No diploma found with id = {id}";
                    return response;
                }
                response.Result = _mapper.Map<LawyerDiplomaDTO>(diploma);
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            return response;
        }

        public async Task<ResponseDto<IEnumerable<LawyerDiplomaDTO>>> GetDiplomasByLawyerIdAsync(int lawyerId, bool includeDeleted = false)
        {
            var response = new ResponseDto<IEnumerable<LawyerDiplomaDTO>>();
            try
            {
                var diplomas = (await _diplomaRepository.GetAllAsync()).Where(d => d.LawyerId == lawyerId);
                if (!includeDeleted)
                {
                    diplomas = diplomas.Where(d => !d.IsDeleted);
                }
                response.Result = _mapper.Map<IEnumerable<LawyerDiplomaDTO>>(diplomas);
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            return response;
        }

        public async Task<ResponseDto<LawyerDiplomaDTO>> CreateDiplomaAsync(int lawyerId, LawyerDiplomaCreateDto diplomaDto)
        {
            var response = new ResponseDto<LawyerDiplomaDTO>();
            try
            {
                var diploma = _mapper.Map<LawyerDiploma>(diplomaDto);
                diploma.LawyerId = lawyerId;
                diploma.CreatedAt = DateTime.Now;
                diploma.UpdatedAt = DateTime.Now;
                diploma.IsDeleted = false;

                await _diplomaRepository.AddAsync(diploma);

                response.Result = _mapper.Map<LawyerDiplomaDTO>(diploma);
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            return response;
        }

        public async Task<ResponseDto<LawyerDiplomaDTO>> UpdateDiplomaAsync(int id, LawyerDiplomaUpdateDto diplomaDto)
        {
            var response = new ResponseDto<LawyerDiplomaDTO>();
            try
            {
                var existingDiploma = await _diplomaRepository.GetByIdAsync(id);
                if (existingDiploma == null)
                {
                    response.IsSuccess = false;
                    response.Message = $"No diploma found with id = {id}";
                    return response;
                }

                _mapper.Map(diplomaDto, existingDiploma);
                existingDiploma.UpdatedAt = DateTime.Now;

                await _diplomaRepository.UpdateAsync(existingDiploma);

                response.Result = _mapper.Map<LawyerDiplomaDTO>(existingDiploma);
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            return response;
        }

        public async Task<ResponseDto<bool>> DeleteDiplomaAsync(int id)
        {
            var response = new ResponseDto<bool>();
            try
            {
                var diploma = await _diplomaRepository.GetByIdAsync(id);
                if (diploma == null)
                {
                    response.IsSuccess = false;
                    response.Message = $"No diploma found with id = {id}";
                    return response;
                }

                await _diplomaRepository.DeleteAsync(id);
                response.Result = true;
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            return response;
        }

        public async Task<ResponseDto<bool>> DeleteDiplomaHardAsync(int id)
        {
            var response = new ResponseDto<bool>();
            try
            {
                var diploma = await _diplomaRepository.GetByIdAsync(id);
                if (diploma == null)
                {
                    response.IsSuccess = false;
                    response.Message = $"No diploma found with id = {id}";
                    return response;
                }
                await _diplomaRepository.DeleteAsync(id);
                response.Result = true;
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            return response;
        }
    }
} 