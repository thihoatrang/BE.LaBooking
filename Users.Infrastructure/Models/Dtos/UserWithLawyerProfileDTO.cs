namespace Users.Infrastructure.Models.Dtos
{
    public class UserWithLawyerProfileDTO
    {
        public UserDTO User { get; set; }
        public LawyerProfileDTO? LawyerProfile { get; set; }

    }

    public class UpdateUserWithLawyerProfileDTO
    {
        public UpdateUserDTO User { get; set; }
        public UpdateLawyerProfileDTO? LawyerProfile { get; set; }
    }
}
