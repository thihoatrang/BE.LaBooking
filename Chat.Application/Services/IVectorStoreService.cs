namespace Chat.Application.Services;

public interface IVectorStoreService
{
    Task IndexDocumentAsync(string id, string content, Dictionary<string, object> metadata, CancellationToken cancellationToken = default);
    Task<List<(string Id, string Content, Dictionary<string, object> Metadata, float Score)>> SearchSimilarAsync(string query, int topK = 5, CancellationToken cancellationToken = default);
    Task DeleteDocumentAsync(string id, CancellationToken cancellationToken = default);
    Task ClearAllAsync(CancellationToken cancellationToken = default);
}
