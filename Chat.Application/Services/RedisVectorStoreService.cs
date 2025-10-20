using StackExchange.Redis;
using System.Text.Json;

namespace Chat.Application.Services;

public class RedisVectorStoreService : IVectorStoreService
{
    private readonly IDatabase _database;
    private readonly IEmbeddingService _embeddingService;
    private const string VECTOR_INDEX = "knowledge_vectors";
    private const string METADATA_PREFIX = "meta:";

    public RedisVectorStoreService(IConnectionMultiplexer redis, IEmbeddingService embeddingService)
    {
        _database = redis.GetDatabase();
        _embeddingService = embeddingService;
    }

    public async Task IndexDocumentAsync(string id, string content, Dictionary<string, object> metadata, CancellationToken cancellationToken = default)
    {
        try
        {
            // Generate embedding for the content
            var embedding = await _embeddingService.GenerateEmbeddingAsync(content, cancellationToken);
            
            // Store metadata
            var metadataJson = JsonSerializer.Serialize(metadata);
            await _database.StringSetAsync($"{METADATA_PREFIX}{id}", metadataJson);
            
            // Store vector in Redis (using simple JSON storage for now)
            var vectorData = new
            {
                id,
                content,
                embedding = embedding,
                metadata
            };
            
            var vectorJson = JsonSerializer.Serialize(vectorData);
            await _database.HashSetAsync(VECTOR_INDEX, id, vectorJson);
        }
        catch (Exception ex)
        {
            // Log error but don't throw to avoid breaking the chat service
            Console.WriteLine($"Error indexing document {id}: {ex.Message}");
        }
    }

    public async Task<List<(string Id, string Content, Dictionary<string, object> Metadata, float Score)>> SearchSimilarAsync(string query, int topK = 5, CancellationToken cancellationToken = default)
    {
        try
        {
            // Generate embedding for the query
            var queryEmbedding = await _embeddingService.GenerateEmbeddingAsync(query, cancellationToken);
            
            // Get all stored vectors
            var allVectors = await _database.HashGetAllAsync(VECTOR_INDEX);
            var results = new List<(string Id, string Content, Dictionary<string, object> Metadata, float Score)>();
            
            foreach (var vector in allVectors)
            {
                try
                {
                    var vectorData = JsonSerializer.Deserialize<JsonElement>(vector.Value!);
                    var embedding = vectorData.GetProperty("embedding").EnumerateArray().Select(x => x.GetSingle()).ToArray();
                    var content = vectorData.GetProperty("content").GetString() ?? "";
                    var metadata = JsonSerializer.Deserialize<Dictionary<string, object>>(vectorData.GetProperty("metadata").GetRawText()) ?? new();
                    
                    // Calculate cosine similarity
                    var similarity = CalculateCosineSimilarity(queryEmbedding, embedding);
                    
                    results.Add((vector.Name!, content, metadata, similarity));
                }
                catch
                {
                    // Skip invalid vectors
                    continue;
                }
            }
            
            // Sort by similarity and return top K
            return results.OrderByDescending(x => x.Score).Take(topK).ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error searching vectors: {ex.Message}");
            return new List<(string Id, string Content, Dictionary<string, object> Metadata, float Score)>();
        }
    }

    public async Task DeleteDocumentAsync(string id, CancellationToken cancellationToken = default)
    {
        await _database.HashDeleteAsync(VECTOR_INDEX, id);
        await _database.KeyDeleteAsync($"{METADATA_PREFIX}{id}");
    }

    public async Task ClearAllAsync(CancellationToken cancellationToken = default)
    {
        await _database.KeyDeleteAsync(VECTOR_INDEX);
        // Note: For production, consider using SCAN instead of KEYS for better performance
        var server = _database.Multiplexer.GetServer(_database.Multiplexer.GetEndPoints().First());
        var keys = server.Keys(pattern: $"{METADATA_PREFIX}*");
        foreach (var key in keys)
        {
            await _database.KeyDeleteAsync(key);
        }
    }

    private static float CalculateCosineSimilarity(float[] vectorA, float[] vectorB)
    {
        if (vectorA.Length != vectorB.Length)
            return 0f;

        float dotProduct = 0f;
        float magnitudeA = 0f;
        float magnitudeB = 0f;

        for (int i = 0; i < vectorA.Length; i++)
        {
            dotProduct += vectorA[i] * vectorB[i];
            magnitudeA += vectorA[i] * vectorA[i];
            magnitudeB += vectorB[i] * vectorB[i];
        }

        magnitudeA = (float)Math.Sqrt(magnitudeA);
        magnitudeB = (float)Math.Sqrt(magnitudeB);

        if (magnitudeA == 0f || magnitudeB == 0f)
            return 0f;

        return dotProduct / (magnitudeA * magnitudeB);
    }
}
