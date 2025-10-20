using System.ComponentModel.DataAnnotations;

namespace Lawyers.Infrastructure.Models.Dtos
{
    public class ActivateWorkSlotDto
    {
        [Required]
        public string Slot { get; set; }
        [Required]
        public string DayOfWeek { get; set; }
        [Required]
        public int LawyerId { get; set; }
    }
} 