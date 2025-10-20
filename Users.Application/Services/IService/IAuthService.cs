using Users.Infrastructure.Models.Dtos;

namespace Users.Application.Services.IService
{
    public interface IAuthService
    {
        Task<AuthResponseDTO> LoginAsync(LoginRequestDTO loginRequest);
        Task<AuthResponseDTO> RegisterAsync(RegisterRequestDTO registerRequest);
        Task<ResponseDto<UserDTO>> UpdateUserAsync(int userId, UpdateUserDTO updateUserDto);
        Task<AuthResponseDTO> GoogleLoginAsync(GoogleAuthDTO googleAuth);
        Task<AuthResponseDTO> VerifyOtpAsync(VerifyOtpDTO verifyOtp);
        Task<ResponseDto<bool>> DeleteUserAsync(string email);
        Task<ResponseDto<bool>> ForgotPasswordAsync(ForgotPasswordDTO forgotPassword);
        Task<ResponseDto<bool>> ResetPasswordAsync(ResetPasswordDTO resetPassword);
        Task<ResponseDto<bool>> ChangePasswordAsync(ChangePasswordDTO changePassword);
        Task<UserDTO?> GetUserByEmailAsync(string email);
    }
}