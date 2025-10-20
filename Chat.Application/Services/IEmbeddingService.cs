namespace Chat.Application.Services;

public interface IEmbeddingService
{
    Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default);
    Task<List<(string Text, float[] Embedding)>> GenerateEmbeddingsAsync(List<string> texts, CancellationToken cancellationToken = default);
}
