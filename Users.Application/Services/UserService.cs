using AutoMapper;

using Microsoft.EntityFrameworkCore;
using Users.Infrastructure.Data;
using Users.Infrastructure.Models;
using Users.Infrastructure.Models.Dtos;
using Users.Infrastructure.Repository;
using Users.Application.Services.IService;

namespace Users.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public UserService(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<ResponseDto<IEnumerable<UserDTO>>> GetAllUsersAsync(bool includeInactive = false)
        {
            var response = new ResponseDto<IEnumerable<UserDTO>>();
            try
            {
                var users = await _userRepository.GetAllAsync();
                if (!includeInactive)
                {
                    users = users.Where(u => u.IsActive);
                }
                response.Result = _mapper.Map<IEnumerable<UserDTO>>(users);
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            return response;
        }

        public async Task<ResponseDto<UserDTO>> GetUserByIdAsync(int id)
        {
            var response = new ResponseDto<UserDTO>();
            try
            {
                var user = await _userRepository.GetByIdAsync(id);
                if (user == null)
                {
                    response.IsSuccess = false;
                    response.Message = $"No user found with id = {id}";
                    return response;
                }
                response.Result = _mapper.Map<UserDTO>(user);
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            return response;
        }

        public async Task<ResponseDto<UserDTO>> CreateUserAsync(UserDTO userDto)
        {
            var response = new ResponseDto<UserDTO>();
            try
            {
                var userEntity = _mapper.Map<User>(userDto);
                userEntity.IsActive = true;
                await _userRepository.AddAsync(userEntity);
                response.Result = _mapper.Map<UserDTO>(userEntity);
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            return response;
        }

        public async Task<ResponseDto<bool>> UpdateUserAsync(int id, UserDTO userDto)
        {
            var response = new ResponseDto<bool>();
            try
            {
                var existing = await _userRepository.GetByIdAsync(id);
                if (existing == null)
                {
                    response.IsSuccess = false;
                    response.Message = $"No user found with id = {id}";
                    response.Result = false;
                    return response;
                }
                _mapper.Map(userDto, existing);
                await _userRepository.UpdateAsync(existing);
                response.Result = true;
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
                response.Result = false;
            }
            return response;
        }

        public async Task<ResponseDto<bool>> SoftDeleteUserAsync(int id)
        {
            var response = new ResponseDto<bool>();
            try
            {
                var existing = await _userRepository.GetByIdAsync(id);
                if (existing == null)
                {
                    response.IsSuccess = false;
                    response.Message = $"No user found with id = {id}";
                    response.Result = false;
                    return response;
                }
                existing.IsActive = false;
                await _userRepository.UpdateAsync(existing);
                response.Result = true;
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
                response.Result = false;
            }
            return response;
        }

        public async Task<ResponseDto<bool>> HardDeleteUserAsync(int id)
        {
            var response = new ResponseDto<bool>();
            try
            {
                var existing = await _userRepository.GetByIdAsync(id);
                if (existing == null)
                {
                    response.IsSuccess = false;
                    response.Message = $"No user found with id = {id}";
                    response.Result = false;
                    return response;
                }
                await _userRepository.DeleteAsync(id);
                response.Result = true;
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
                response.Result = false;
            }
            return response;
        }

        public async Task<ResponseDto<bool>> RestoreUserAsync(int id)
        {
            var response = new ResponseDto<bool>();
            try
            {
                var existing = await _userRepository.GetByIdAsync(id);
                if (existing == null)
                {
                    response.IsSuccess = false;
                    response.Message = $"No user found with id = {id}";
                    response.Result = false;
                    return response;
                }
                existing.IsActive = true;
                await _userRepository.UpdateAsync(existing);
                response.Result = true;
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
                response.Result = false;
            }
            return response;
        }
    }
}