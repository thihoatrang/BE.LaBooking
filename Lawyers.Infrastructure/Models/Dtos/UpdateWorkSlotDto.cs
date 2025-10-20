using System.ComponentModel.DataAnnotations;

namespace Lawyers.Infrastructure.Models.Dtos
{
    public class UpdateWorkSlotDto
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public string DayOfWeek { get; set; }

        [Required]
        public string Slot { get; set; }

        [Required]
        public bool IsActive { get; set; }
    }

    public class UpdateWorkSlotDtoNoId
    {
        public string DayOfWeek { get; set; }
        public string Slot { get; set; }
        public bool IsActive { get; set; }
    }
} 