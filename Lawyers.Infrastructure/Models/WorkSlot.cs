using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lawyers.Infrastructure.Models
{
    public class WorkSlot
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int LawyerId { get; set; }
        
        [ForeignKey("LawyerId")]
        public LawyerProfile Lawyer { get; set; }

        [Required]
        public string DayOfWeek { get; set; }

        [Required]
        public string Slot { get; set; }

        [Required]
        public bool IsActive { get; set; }
    }
} 