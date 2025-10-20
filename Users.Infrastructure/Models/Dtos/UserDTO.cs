namespace Users.Infrastructure.Models.Dtos
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

    public class UpdateUserDTO
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public bool IsActive { get; set; }
    }
}
