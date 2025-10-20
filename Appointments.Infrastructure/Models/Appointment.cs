using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Appointments.Infrastructure.Models.Enums;

namespace Appointments.Infrastructure.Models
{
    public class Appointment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int LawyerId { get; set; }

        [Required]
       
        public DateTime ScheduledAt { get; set; }

        [Required]
        public string Slot { get; set; }

        [Required]
    
        public DateTime CreateAt { get; set; }

        [Required]
        public AppointmentStatus Status { get; set; }

        [Required]
        public bool IsDel { get; set; }

        public string? Note { get; set; }

        [Required]
        public string Spec { get; set; }

        [Required]
        public List<string> Services { get; set; }
    }
} 