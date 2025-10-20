using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;

namespace Chat.Application.Services;

public class SimpleEmbeddingService : IEmbeddingService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public SimpleEmbeddingService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text))
            return new float[384]; // Default dimension for sentence-transformers

        try
        {
            // Use a simple text embedding service (you can replace with OpenAI embeddings or local model)
            var apiKey = _configuration["OpenAI:ApiKey"];
            var baseUrl = _configuration["OpenAI:BaseUrl"] ?? "https://api.openai.com";
            
            var requestBody = new
            {
                input = text,
                model = "text-embedding-3-small" // or "text-embedding-ada-002"
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
            
            using var response = await _httpClient.PostAsync($"{baseUrl}/v1/embeddings", content, cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                var doc = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
                var embedding = doc.RootElement.GetProperty("data")[0].GetProperty("embedding");
                return embedding.EnumerateArray().Select(x => x.GetSingle()).ToArray();
            }
        }
        catch
        {
            // Fallback to simple hash-based embedding
        }

        // Fallback: create a simple hash-based embedding
        return CreateHashEmbedding(text);
    }

    public async Task<List<(string Text, float[] Embedding)>> GenerateEmbeddingsAsync(List<string> texts, CancellationToken cancellationToken = default)
    {
        var results = new List<(string Text, float[] Embedding)>();
        
        foreach (var text in texts)
        {
            var embedding = await GenerateEmbeddingAsync(text, cancellationToken);
            results.Add((text, embedding));
        }
        
        return results;
    }

    private static float[] CreateHashEmbedding(string text)
    {
        // Simple hash-based embedding for fallback
        var hash = text.GetHashCode();
        var random = new Random(hash);
        var embedding = new float[384];
        
        for (int i = 0; i < embedding.Length; i++)
        {
            embedding[i] = (float)(random.NextDouble() * 2 - 1); // -1 to 1
        }
        
        return embedding;
    }
}
