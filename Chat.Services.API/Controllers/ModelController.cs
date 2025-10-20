using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Chat.Services.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ModelController : ControllerBase
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public ModelController(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    [HttpGet("list-gemini")]
    public async Task<ActionResult> ListGeminiModelsAsync(CancellationToken cancellationToken)
    {
        try
        {
            var apiKey = _configuration["OpenAI:ApiKey"];
            var baseUrl = "https://generativelanguage.googleapis.com";
            
            _httpClient.BaseAddress = new Uri(baseUrl);
            _httpClient.DefaultRequestHeaders.Authorization = null;
            
            var response = await _httpClient.GetAsync($"/v1beta/models?key={Uri.EscapeDataString(apiKey ?? string.Empty)}", cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                var doc = JsonDocument.Parse(content);
                return Ok(doc.RootElement);
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync(cancellationToken);
                return BadRequest(new { error, statusCode = response.StatusCode });
            }
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("test-gemini")]
    public async Task<ActionResult> TestGeminiAsync(CancellationToken cancellationToken)
    {
        try
        {
            var apiKey = _configuration["OpenAI:ApiKey"];
            var baseUrl = "https://generativelanguage.googleapis.com";
            
            _httpClient.BaseAddress = new Uri(baseUrl);
            _httpClient.DefaultRequestHeaders.Authorization = null;
            
            // Test with a simple request
            var testBody = new
            {
                contents = new object[]
                {
                    new { 
                        parts = new object[] { 
                            new { text = "Hello, how are you?" } 
                        } 
                    }
                }
            };
            
            var json = JsonSerializer.Serialize(testBody);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync($"/v1beta/models/gemini-1.5-flash:generateContent?key={Uri.EscapeDataString(apiKey ?? string.Empty)}", content, cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                return Ok(new { success = true, response = responseContent });
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync(cancellationToken);
                return BadRequest(new { error, statusCode = response.StatusCode });
            }
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
