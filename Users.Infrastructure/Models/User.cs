using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Users.Infrastructure.Models
{  
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // ID tự tăng
        
        public int Id { get; set; }

        
        public string FullName { get; set; }

       
        public string Email { get; set; }

        
        public string Password { get; set; }

       
        public string PhoneNumber { get; set; }

   
        public string Role { get; set; } // "Customer", "Lawyer", "Admin", "Guest"

        public bool IsActive { get; set; }
    }
}
