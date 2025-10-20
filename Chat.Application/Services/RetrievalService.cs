using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;

namespace Chat.Application.Services;

public class RetrievalService : IRetrievalService
{
	private readonly IVectorStoreService _vectorStore;
	private readonly HttpClient _httpClient;
	private readonly IConfiguration _configuration;

	public RetrievalService(IVectorStoreService vectorStore, HttpClient httpClient, IConfiguration configuration)
	{
		_vectorStore = vectorStore;
		_httpClient = httpClient;
		_configuration = configuration;
	}

	public async Task<string> BuildContextAsync(string? userId, CancellationToken cancellationToken)
	{
		var contextParts = new List<string>();

		// Get user-specific information if userId provided
		if (!string.IsNullOrWhiteSpace(userId))
		{
			try
			{
				var usersApi = _configuration["ServiceUrls:UsersAPI"] ?? "http://users-service";
				var profile = await _httpClient.GetFromJsonAsync<object>($"{usersApi}/api/users/{userId}", cancellationToken);
				contextParts.Add($"Thông tin người dùng: {profile}");
			}
			catch 
			{ 
				// Fallback to vector search for user info
				var userResults = await _vectorStore.SearchSimilarAsync($"user {userId}", 2, cancellationToken);
				if (userResults.Any())
				{
					contextParts.Add($"Thông tin người dùng: {string.Join("; ", userResults.Select(r => r.Content))}");
				}
			}
		}

		// Search for relevant knowledge using vector similarity
		var searchQuery = userId != null ? $"luật sư dịch vụ tư vấn pháp lý" : "luật sư dịch vụ tư vấn pháp lý";
		var knowledgeResults = await _vectorStore.SearchSimilarAsync(searchQuery, 5, cancellationToken);
		
		if (knowledgeResults.Any())
		{
			var knowledgeContext = string.Join("\n\n", knowledgeResults.Select(r => r.Content));
			contextParts.Add($"Thông tin dịch vụ và luật sư:\n{knowledgeContext}");
		}
		else
		{
			// Fallback to basic API calls if vector search fails
			contextParts.Add(await GetFallbackContextAsync(cancellationToken));
		}

		return string.Join("\n\n", contextParts);
	}

	private async Task<string> GetFallbackContextAsync(CancellationToken cancellationToken)
	{
		var contextParts = new List<string>();

		try
		{
			var lawyersApi = _configuration["ServiceUrls:LawyersAPI"] ?? "http://lawyers-service";
			var list = await _httpClient.GetStringAsync($"{lawyersApi}/api/lawyers", cancellationToken);
			contextParts.Add($"Dịch vụ/luật sư: {Truncate(list, 800)}");
		}
		catch 
		{ 
			contextParts.Add("Danh mục dịch vụ luật sư hiện có: khám phá tại /api/lawyers");
		}

		return string.Join("\n", contextParts);
	}

	private static string Truncate(string value, int max)
	{
		if (string.IsNullOrEmpty(value)) return value;
		return value.Length <= max ? value : value.Substring(0, max);
	}
}


