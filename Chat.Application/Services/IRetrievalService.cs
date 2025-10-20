namespace Chat.Application.Services;

public interface IRetrievalService
{
	Task<string> BuildContextAsync(string? userId, CancellationToken cancellationToken);
}


