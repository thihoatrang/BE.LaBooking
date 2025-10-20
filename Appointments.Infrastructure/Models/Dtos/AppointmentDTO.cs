using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Appointments.Infrastructure.Models.Enums;

namespace Appointments.Infrastructure.Models.DTOs
{
    public class AppointmentDTO
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int LawyerId { get; set; }
        public DateTime ScheduledAt { get; set; }
        public string Slot { get; set; }
        public DateTime CreateAt { get; set; }
        public AppointmentStatus Status { get; set; }
        public bool IsDel { get; set; }
        public string? Note { get; set; }
        public string Spec { get; set; }
        public List<string> Services { get; set; }     
    }  
  
}

