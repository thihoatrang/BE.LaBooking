namespace Appointments.Infrastructure.Models.Dtos
{
    public class ResponseDto<T>
    {
        public bool IsSuccess { get; set; } = true;
        public T? Result { get; set; }
        public string? DisplayMessage { get; set; }
        public List<string>? ErrorMessages { get; set; }
    }
} 