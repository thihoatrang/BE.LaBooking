namespace Chat.Application.Services;

public interface IKnowledgeService
{
    Task IndexLawyersDataAsync(CancellationToken cancellationToken = default);
    Task IndexUsersDataAsync(CancellationToken cancellationToken = default);
    Task IndexAppointmentsDataAsync(CancellationToken cancellationToken = default);
    Task IndexCustomDataAsync(string content, string source, Dictionary<string, object>? metadata = null, CancellationToken cancellationToken = default);
    Task RebuildKnowledgeBaseAsync(CancellationToken cancellationToken = default);
}
