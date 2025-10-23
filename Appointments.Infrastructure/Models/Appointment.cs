namespace Appointments.Infrastructure.Models;

public partial class Appointment
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int LawyerId { get; set; }

    public DateOnly? ScheduledAt { get; set; }

    public string? Slot { get; set; }

    public DateOnly? CreateAt { get; set; }

    public int Status { get; set; }

    public bool IsDel { get; set; }

    public string? Note { get; set; }

    public string Spec { get; set; } = null!;

    public string Services { get; set; } = null!;
}
