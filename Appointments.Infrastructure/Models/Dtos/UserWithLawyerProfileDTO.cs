using Appointments.Infrastructure.Models.DTOs;

namespace Appointments.Infrastructure.Models.Dtos
{
    public class UserWithLawyerProfileDTO
    {
        public UserDTO User { get; set; }
        public LawyerProfileDTO? LawyerProfile { get; set; }
    }
}
