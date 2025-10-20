using System.Net.Http.Json;
using System.Text.Json;

using Users.Infrastructure.Models.Dtos;

public class LawyerProfileApiClient
{
    private readonly HttpClient _httpClient;

    public LawyerProfileApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<LawyerProfileDTO?> GetByUserIdAsync(int userId)
    {
        var response = await _httpClient.GetAsync($"/api/lawyer/GetProfileByUserId/{userId}");
        if (!response.IsSuccessStatusCode) return null;

        var json = await response.Content.ReadAsStringAsync();
        var wrapper = JsonSerializer.Deserialize<ResponseDto<LawyerProfileDTO>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        return wrapper?.Result;
       
    }

    public async Task<bool> UpdateLawyerProfileAsync(int id, LawyerProfileDTO dto)
    {
        var response = await _httpClient.PutAsJsonAsync($"/api/lawyer/UpdateLawyerProfile/{id}", dto);
        return response.IsSuccessStatusCode;
    }
}