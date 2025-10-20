using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Chat.Infrastructure.Models.Dtos;
using Microsoft.Extensions.Configuration;

namespace Chat.Application.Services;

public class OpenAIChatService : IChatService
{
	private readonly HttpClient _httpClient;
	private readonly string _model;
	private readonly bool _hasApiKey;
	private readonly string _baseUrl;
	private readonly string _apiKey;

	public OpenAIChatService(HttpClient httpClient, IConfiguration configuration)
	{
		_httpClient = httpClient;
		_baseUrl = configuration["Gemini:BaseUrl"] ?? "https://generativelanguage.googleapis.com";
		_apiKey = configuration["Gemini:ApiKey"] ?? string.Empty;
		_hasApiKey = !string.IsNullOrWhiteSpace(_apiKey);
		_model = configuration["Gemini:Model"] ?? "gemini-2.5-flash";
		
		// Gemini uses API key as query parameter, not in headers
		_httpClient.DefaultRequestHeaders.Authorization = null;
		_httpClient.BaseAddress = new Uri(_baseUrl);
	}

	public async Task<ChatResponseDto> GenerateAnswerAsync(string userMessage, string knowledgeContext, CancellationToken cancellationToken)
	{
		if (!_hasApiKey)
		{
			return new ChatResponseDto
			{
				Answer = "Hệ thống chat AI chưa được cấu hình Gemini API key. Vui lòng đặt biến môi trường Gemini__ApiKey hoặc cấu hình trong appsettings.json để bật trả lời AI.",
				Sources = new List<string> { "/config/Gemini__ApiKey" }
			};
		}

        var systemPrompt =
    "Bạn là trợ lý AI tư vấn về luật sư và dịch vụ pháp lý của hệ thống. " +
    "Luôn dựa **chỉ** vào ngữ cảnh được cung cấp, không bịa thông tin. " +
    "Trả lời bằng tiếng Việt, súc tích, dễ đọc. " +
    "KHÔNG sử dụng bất kỳ ký tự định dạng nào như **, *, #, -, hoặc Markdown. " +
    "Chỉ dùng văn bản thuần (plain text). " +
    "Để xuống dòng, hãy dùng ký tự xuống dòng tự nhiên (enter). " +
    "Khi liệt kê luật sư, viết từng mục trên một dòng mới, không dùng gạch đầu dòng. " +
    "Luôn kết thúc bằng một câu hỏi gợi mở để hỗ trợ người dùng tiếp.";

        try
		{
			HttpResponseMessage response;
			if (_model.StartsWith("gemini", StringComparison.OrdinalIgnoreCase))
			{
				// Try Gemini first
				try
				{
					var geminiApiKey = _apiKey;
					var url = $"/v1/models/{_model}:generateContent?key={Uri.EscapeDataString(geminiApiKey ?? string.Empty)}";
					var geminiBody = new
					{
						contents = new object[]
						{
							new { 
								parts = new object[] { 
									new { text = $"{systemPrompt}\n\nNgữ cảnh: {knowledgeContext}\n\nCâu hỏi: {userMessage}" } 
								} 
							}
						},
						generationConfig = new { temperature = 0.2 },
						safetySettings = new object[] { }
					};
					var gjson = JsonSerializer.Serialize(geminiBody);
					var gcontent = new StringContent(gjson, Encoding.UTF8, "application/json");
					response = await _httpClient.PostAsync(url, gcontent, cancellationToken);
				}
				catch (Exception geminiEx)
				{
					Console.WriteLine($"Gemini failed, falling back to OpenAI: {geminiEx.Message}");
					// Fallback to OpenAI
					_httpClient.BaseAddress = new Uri("https://generativelanguage.googleapis.com");
					_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
					
					var requestBody = new
					{
						model = "gemini-1.5-flash",
						messages = new object[]
						{
							new { role = "system", content = systemPrompt },
							new { role = "system", content = $"Ngữ cảnh: {knowledgeContext}" },
							new { role = "user", content = userMessage }
						},
						temperature = 0.2
					};
					var json = JsonSerializer.Serialize(requestBody);
					var content = new StringContent(json, Encoding.UTF8, "application/json");
					response = await _httpClient.PostAsync("/v1/chat/completions", content, cancellationToken);
				}
			}
			else
			{
				// OpenAI chat completions
				var requestBody = new
				{
					model = _model,
					messages = new object[]
					{
						new { role = "system", content = systemPrompt },
						new { role = "system", content = $"Ngữ cảnh: {knowledgeContext}" },
						new { role = "user", content = userMessage }
					},
					temperature = 0.2
				};
				var json = JsonSerializer.Serialize(requestBody);
				var content = new StringContent(json, Encoding.UTF8, "application/json");
				response = await _httpClient.PostAsync("/v1/chat/completions", content, cancellationToken);
			}
			if (!response.IsSuccessStatusCode)
			{
				var err = await response.Content.ReadAsStringAsync(cancellationToken);
				return new ChatResponseDto
				{
					Answer = $"OpenAI trả về lỗi {(int)response.StatusCode} {response.StatusCode}. Vui lòng kiểm tra OpenAI__ApiKey/Model. Chi tiết: {Truncate(err, 500)}",
					Sources = new List<string> { "openai" }
				};
			}
			using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
			var doc = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
			string answer = string.Empty;
			
			// Check if response is from Gemini or OpenAI
			if (doc.RootElement.TryGetProperty("candidates", out var candidates))
			{
				// Gemini response
				if (candidates.GetArrayLength() > 0)
				{
					var candidate = candidates[0];
					var content = candidate.GetProperty("content");
					var parts = content.GetProperty("parts");
					if (parts.GetArrayLength() > 0)
					{
						answer = parts[0].GetProperty("text").GetString() ?? string.Empty;
					}
				}
			}
			else if (doc.RootElement.TryGetProperty("choices", out var choices))
			{
				// OpenAI response
				answer = choices[0].GetProperty("message").GetProperty("content").GetString() ?? string.Empty;
			}
			
			return new ChatResponseDto { Answer = answer };
		}
		catch (Exception ex)
		{
			return new ChatResponseDto
			{
				Answer = $"Không thể gọi OpenAI: {ex.Message}",
				Sources = new List<string> { "openai" }
			};
		}
	}

	private static string Truncate(string value, int max)
	{
		if (string.IsNullOrEmpty(value)) return value;
		return value.Length <= max ? value : value.Substring(0, max);
	}
}


