namespace Chat.Application.Services;

public class MockVectorStoreService : IVectorStoreService
{
    public Task IndexDocumentAsync(string id, string content, Dictionary<string, object> metadata, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"Mock: Would index document {id}");
        return Task.CompletedTask;
    }

    public Task<List<(string Id, string Content, Dictionary<string, object> Metadata, float Score)>> SearchSimilarAsync(string query, int topK = 5, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"Mock: Would search for '{query}' (top {topK})");
        // Return some mock data
        return Task.FromResult(new List<(string Id, string Content, Dictionary<string, object> Metadata, float Score)>
        {
            ("mock_1", "Luật sư Nguyễn Văn A chuyên về dân sự, có 10 năm kinh nghiệm", 
             new Dictionary<string, object> { ["type"] = "lawyer" }, 0.9f),
            ("mock_2", "Dịch vụ tư vấn pháp luật bao gồm: dân sự, hình sự, thương mại", 
             new Dictionary<string, object> { ["type"] = "service" }, 0.8f)
        });
    }

    public Task DeleteDocumentAsync(string id, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"Mock: Would delete document {id}");
        return Task.CompletedTask;
    }

    public Task ClearAllAsync(CancellationToken cancellationToken = default)
    {
        Console.WriteLine("Mock: Would clear all documents");
        return Task.CompletedTask;
    }
}
