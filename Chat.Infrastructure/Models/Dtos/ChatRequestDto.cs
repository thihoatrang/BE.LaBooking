namespace Chat.Infrastructure.Models.Dtos;

public class ChatRequestDto
{
	public string Message { get; set; } = string.Empty;
	public string? UserId { get; set; }
}


