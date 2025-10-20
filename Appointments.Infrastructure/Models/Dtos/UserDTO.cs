namespace Appointments.Infrastructure.Models.DTOs
{
    public class UserDTO
    {
        public int Id { get; set; }


        public string FullName { get; set; }


        public string Email { get; set; }


        public string Password { get; set; }


        public string PhoneNumber { get; set; }


        public string Role { get; set; } // "Customer", "Lawyer", "Admin", "Guest"

        public bool IsActive { get; set; }

    }
}
