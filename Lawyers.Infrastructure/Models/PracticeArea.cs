using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lawyers.Infrastructure.Models
{
    public class PracticeArea
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Code { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        // Navigation properties
        public virtual ICollection<LawyerPracticeArea> LawyerPracticeAreas { get; set; } = new List<LawyerPracticeArea>();
        public virtual ICollection<Service> Services { get; set; } = new List<Service>();
    }
}
