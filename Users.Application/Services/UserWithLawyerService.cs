using Users.Infrastructure.Models.Dtos;

using Users.Application.Services.IService;
// USer - Lawyer (REF: UserID)
public class UserWithLawyerProfileService : IUserWithLawyerProfileService
{
    private readonly IUserService _userService;
    private readonly LawyerProfileApiClient _lawyerApiClient;

    public UserWithLawyerProfileService(IUserService userService, LawyerProfileApiClient lawyerApiClient)
    {
        _userService = userService;
        _lawyerApiClient = lawyerApiClient;
    }

    public async Task<IEnumerable<UserWithLawyerProfileDTO>> GetAllUsersWithLawyerProfileAsync()
    {
        var usersResponse = await _userService.GetAllUsersAsync();
        var result = new List<UserWithLawyerProfileDTO>();

        foreach (var user in usersResponse.Result)
        {
            LawyerProfileDTO? lawyerProfile = null;
            if (user.Role == "Lawyer")
            {
                lawyerProfile = await _lawyerApiClient.GetByUserIdAsync(user.Id);
            }
            result.Add(new UserWithLawyerProfileDTO
            {
                User = user,
                LawyerProfile = lawyerProfile
            });
        }
        return result;
    }

    public async Task<UserWithLawyerProfileDTO?> GetUsersWithLawyerProfileByIdAsync(int userId)
    {
        var userResponse = await _userService.GetUserByIdAsync(userId);
        var user = userResponse.Result;
        if (user == null)
            return null;

        LawyerProfileDTO? lawyerProfile = null;
        if (user.Role == "Lawyer")
        {
            lawyerProfile = await _lawyerApiClient.GetByUserIdAsync(user.Id);
        }

        return new UserWithLawyerProfileDTO
        {
            User = user,
            LawyerProfile = lawyerProfile
        };
    }

    public async Task<IEnumerable<UserWithLawyerProfileDTO>> GetUsersWithLawyerProfileOnlyAsync()
    {
        var usersResponse = await _userService.GetAllUsersAsync();
        var result = new List<UserWithLawyerProfileDTO>();

        foreach (var user in usersResponse.Result)
        {
            if (user.Role == "Lawyer")
            {
                var lawyerProfile = await _lawyerApiClient.GetByUserIdAsync(user.Id);
                if (lawyerProfile != null)
                {
                    result.Add(new UserWithLawyerProfileDTO
                    {
                        User = user,
                        LawyerProfile = lawyerProfile
                    });
                }
            }
        }
        return result;
    }

    public async Task<IEnumerable<UserWithLawyerProfileDTO>> GetAllUsersWithLawyerProfileIncludingInactiveAsync()
    {
        var usersResponse = await _userService.GetAllUsersAsync(true); // includeInactive = true
        var result = new List<UserWithLawyerProfileDTO>();

        foreach (var user in usersResponse.Result)
        {
            LawyerProfileDTO? lawyerProfile = null;
            if (user.Role == "Lawyer")
            {
                lawyerProfile = await _lawyerApiClient.GetByUserIdAsync(user.Id);
            }
            result.Add(new UserWithLawyerProfileDTO
            {
                User = user,
                LawyerProfile = lawyerProfile
            });
        }
        return result;
    }

    public async Task<ResponseDto<bool>> UpdateUserWithLawyerProfileAsync(int userId, UpdateUserWithLawyerProfileDTO dto)
    {
        var response = new ResponseDto<bool>();
        // Lấy user hiện tại
        var userUpdateResult = await _userService.GetUserByIdAsync(userId);
        if (userUpdateResult.Result == null)
        {
            response.IsSuccess = false;
            response.Message = $"No user found with id = {userId}";
            response.Result = false;
            return response;
        }
        // Mapping sang UserDTO, giữ nguyên Id và Password
        var userDto = new UserDTO
        {
            Id = userId,
            FullName = dto.User.FullName,
            Email = dto.User.Email,
            PhoneNumber = dto.User.PhoneNumber,
            Role = userUpdateResult.Result.Role, // Giữ nguyên role cũ
            IsActive = dto.User.IsActive,
            Password = userUpdateResult.Result.Password
        };
        var updateUserRes = await _userService.UpdateUserAsync(userId, userDto);
        if (!updateUserRes.IsSuccess)
        {
            response.IsSuccess = false;
            response.Message = updateUserRes.Message;
            response.Result = false;
            return response;
        }
        // Update lawyer profile nếu có
        if (dto.LawyerProfile != null)
        {
            // Lấy profile hiện tại để lấy Id
            var currentProfile = await _lawyerApiClient.GetByUserIdAsync(userId);
            if (currentProfile == null)
            {
                response.IsSuccess = false;
                response.Message = "Lawyer profile not found.";
                response.Result = false;
                return response;
            }
            var updateLawyerDto = new LawyerProfileDTO
            {
                Id = currentProfile.Id, // Lấy id từ DB
                UserId = userId,
                Bio = dto.LawyerProfile.Bio,
                Spec = dto.LawyerProfile.Spec,
                LicenseNum = dto.LawyerProfile.LicenseNum,
                ExpYears = dto.LawyerProfile.ExpYears,
                Description = dto.LawyerProfile.Description,
                Rating = dto.LawyerProfile.Rating,
                PricePerHour = dto.LawyerProfile.PricePerHour,
                Img = dto.LawyerProfile.Img,
                DayOfWeek = dto.LawyerProfile.DayOfWeek,
                WorkTime = dto.LawyerProfile.WorkTime
            };
            var updateLawyerRes = await _lawyerApiClient.UpdateLawyerProfileAsync(currentProfile.Id, updateLawyerDto);
            if (!updateLawyerRes)
            {
                response.IsSuccess = false;
                response.Message = "Failed to update lawyer profile.";
                response.Result = false;
                return response;
            }
        }
        response.IsSuccess = true;
        response.Message = "Update successful";
        response.Result = true;
        return response;
    }
}