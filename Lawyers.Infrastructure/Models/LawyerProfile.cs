using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Lawyers.Infrastructure.Models
{
    public class LawyerProfile
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        //public int Id { get; set; }    
        //public int UserId { get; set; } // Foreign Key tới User
        //public string Bio { get; set; }
        //public List<string> Specialties { get; set; } // VD: "Hôn nhân", "Hình sự"
        //public string LicenseNumber { get; set; }
        //public int ExperienceYears { get; set; }
        //public string Location { get; set; } // Thay bằng Description
        //public double Rating { get; set; }
        public int Id { get; set; }

        public int UserId { get; set; }

        public string Bio { get; set; }

        public List<string> Spec { get; set; } 

        public string LicenseNum { get; set; }

        public int ExpYears { get; set; }

        public string Description { get; set; }

        public double Rating { get; set; }

        public double PricePerHour { get; set; }

        public string Img { get; set; }

        public string DayOfWeek { get; set; }

        public string WorkTime { get; set; }
    }
}
