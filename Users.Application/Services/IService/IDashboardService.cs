using Users.Infrastructure.Models.Dtos;

namespace Users.Application.Services.IService
{
    public interface IDashboardService
    {
        Task<DashboardStatisticsDto> GetDashboardStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null);
    }
}

