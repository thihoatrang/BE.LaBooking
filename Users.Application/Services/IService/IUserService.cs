using System;
using Users.Infrastructure.Models.Dtos;

namespace Users.Application.Services.IService
{
    public interface IUserService
    {
        Task<ResponseDto<IEnumerable<UserDTO>>> GetAllUsersAsync(bool includeInactive = false);
        Task<ResponseDto<UserDTO>> GetUserByIdAsync(int id);
        Task<ResponseDto<UserDTO>> CreateUserAsync(UserDTO userDto);
        Task<ResponseDto<bool>> UpdateUserAsync(int id, UserDTO userDto);
        Task<ResponseDto<bool>> SoftDeleteUserAsync(int id);
        Task<ResponseDto<bool>> HardDeleteUserAsync(int id);
        Task<ResponseDto<bool>> RestoreUserAsync(int id);
    }
}
