using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Lawyers.Infrastructure.Models
{
    public class LawyerProfile
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        public string? Bio { get; set; }

        [Required]
        [MaxLength(50)]
        public string LicenseNum { get; set; }

        [Required]
        public int ExpYears { get; set; }

        public string? Description { get; set; }

        public double Rating { get; set; }

        [Required]
        public double PricePerHour { get; set; }

        public string? Img { get; set; }

        [Required]
        public string DayOfWeek { get; set; }

        [Required]
        public string WorkTime { get; set; }

        [Required]
        public bool IsActive { get; set; } = true;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<LawyerPracticeArea> LawyerPracticeAreas { get; set; } = new List<LawyerPracticeArea>();
    }
}
