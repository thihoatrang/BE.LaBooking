namespace Users.Infrastructure.Models.Dtos
{
    public class ResponseDto<T>
    {
        public bool IsSuccess { get; set; } = true;
        public string Message { get; set; } = string.Empty;
        public T Result { get; set; }
    }
}
