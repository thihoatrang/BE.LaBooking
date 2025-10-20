using Chat.Infrastructure.Models.Dtos;

namespace Chat.Application.Services;

public interface IChatService
{
	Task<ChatResponseDto> GenerateAnswerAsync(string userMessage, string knowledgeContext, CancellationToken cancellationToken);
}


