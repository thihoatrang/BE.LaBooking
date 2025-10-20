using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace API.Gateway.Services
{
    public class ServiceDiscoveryService : IServiceDiscoveryService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ServiceDiscoveryService> _logger;
        private readonly Dictionary<string, string> _serviceUrls;

        public ServiceDiscoveryService(IConfiguration configuration, ILogger<ServiceDiscoveryService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _serviceUrls = new Dictionary<string, string>
            {
                { "UsersService", _configuration["ServiceUrls:UsersAPI"] ?? "https://localhost:7000" },
                { "LawyersService", _configuration["ServiceUrls:LawyersAPI"] ?? "https://localhost:7110" },
                { "AppointmentsService", _configuration["ServiceUrls:AppointmentsAPI"] ?? "https://localhost:7001" }
            };
        }

        public async Task<string> GetServiceUrlAsync(string serviceName)
        {
            if (_serviceUrls.TryGetValue(serviceName, out var url))
            {
                _logger.LogInformation($"Service {serviceName} URL: {url}");
                return url;
            }

            _logger.LogWarning($"Service {serviceName} not found in configuration");
            throw new ArgumentException($"Service {serviceName} not found");
        }

        public async Task<bool> IsServiceHealthyAsync(string serviceName)
        {
            try
            {
                var serviceUrl = await GetServiceUrlAsync(serviceName);
                using var httpClient = new HttpClient();
                httpClient.Timeout = TimeSpan.FromSeconds(5);
                
                var response = await httpClient.GetAsync($"{serviceUrl}/health");
                var isHealthy = response.IsSuccessStatusCode;
                
                _logger.LogInformation($"Service {serviceName} health check: {(isHealthy ? "Healthy" : "Unhealthy")}");
                return isHealthy;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Health check failed for service {serviceName}");
                return false;
            }
        }

        public async Task<IEnumerable<string>> GetAvailableServicesAsync()
        {
            var availableServices = new List<string>();
            
            foreach (var service in _serviceUrls.Keys)
            {
                if (await IsServiceHealthyAsync(service))
                {
                    availableServices.Add(service);
                }
            }
            
            _logger.LogInformation($"Available services: {string.Join(", ", availableServices)}");
            return availableServices;
        }
    }
}
