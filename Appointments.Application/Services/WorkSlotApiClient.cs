using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Appointments.Application.Services
{
    public class WorkSlotApiClient
    {
        private readonly HttpClient _httpClient;
        public WorkSlotApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public async Task DeactivateWorkSlotAsync(string slot, string dayOfWeek, int lawyerId)
        {
            var dto = new
            {
                Slot = slot,
                DayOfWeek = dayOfWeek,
                LawyerId = lawyerId
            };
            var response = await _httpClient.PutAsJsonAsync("api/lawyers/" + lawyerId + "/workslots/deactivate", dto);
            response.EnsureSuccessStatusCode();
        }
        public async Task ActivateWorkSlotAsync(string slot, string dayOfWeek, int lawyerId)
        {
            var dto = new
            {
                Slot = slot,
                DayOfWeek = dayOfWeek,
                LawyerId = lawyerId
            };
            var response = await _httpClient.PutAsJsonAsync($"api/lawyers/{lawyerId}/workslots/activate", dto);
            response.EnsureSuccessStatusCode();
        }
    }
} 