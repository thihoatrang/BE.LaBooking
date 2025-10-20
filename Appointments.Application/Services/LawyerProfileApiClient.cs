using Appointments.Infrastructure.Models.DTOs;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Appointments.Application.Services
{
    public class LawyerProfileApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        public LawyerProfileApiClient(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }
        public async Task<LawyerProfileDTO?> GetLawyerProfileByIdAsync(int lawyerId)
        {
            var response = await _httpClient.GetAsync($"/api/lawyer/GetProfileById/{lawyerId}");
            if (!response.IsSuccessStatusCode) return null;
            var wrapper = await response.Content.ReadFromJsonAsync<ResponseDto<LawyerProfileDTO>>();
            return wrapper?.Result;
        }
    }
} 