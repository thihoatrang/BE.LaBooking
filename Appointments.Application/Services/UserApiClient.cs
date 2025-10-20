using Appointments.Infrastructure.Models.DTOs;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Appointments.Application.Services
{
    public class UserApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        public UserApiClient(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }
        public async Task<UserDTO?> GetUserByIdAsync(int userId)
        {
            var response = await _httpClient.GetAsync($"/api/user/{userId}");
            if (!response.IsSuccessStatusCode) return null;
            var wrapper = await response.Content.ReadFromJsonAsync<ResponseDto<UserDTO>>();
            return wrapper?.Result;
        }
    }
} 