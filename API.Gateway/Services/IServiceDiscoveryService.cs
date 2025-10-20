namespace API.Gateway.Services
{
    public interface IServiceDiscoveryService
    {
        Task<string> GetServiceUrlAsync(string serviceName);
        Task<bool> IsServiceHealthyAsync(string serviceName);
        Task<IEnumerable<string>> GetAvailableServicesAsync();
    }
}
