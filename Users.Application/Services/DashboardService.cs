using Users.Application.Services.IService;
using Users.Infrastructure.Models.Dtos;
using Users.Infrastructure.Repository;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;

namespace Users.Application.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly HttpClient _appointmentsHttpClient;
        private readonly ILogger<DashboardService> _logger;

        public DashboardService(
            IReviewRepository reviewRepository,
            IHttpClientFactory httpClientFactory,
            ILogger<DashboardService> logger)
        {
            _reviewRepository = reviewRepository;
            _appointmentsHttpClient = httpClientFactory.CreateClient("AppointmentsService");
            _logger = logger;
        }

        public async Task<DashboardStatisticsDto> GetDashboardStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            var statistics = new DashboardStatisticsDto
            {
                FromDate = fromDate,
                ToDate = toDate
            };

            try
            {
                // Get statistics from multiple services in parallel
                var paymentStatsTask = GetPaymentStatisticsAsync(fromDate, toDate);
                var appointmentStatsTask = GetAppointmentStatisticsAsync(fromDate, toDate);
                var reviewStatsTask = GetReviewStatisticsAsync(fromDate, toDate);

                await Task.WhenAll(paymentStatsTask, appointmentStatsTask, reviewStatsTask);

                statistics.Payments = await paymentStatsTask;
                statistics.Appointments = await appointmentStatsTask;
                statistics.Reviews = await reviewStatsTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard statistics");
                throw;
            }

            return statistics;
        }

        private async Task<PaymentStatisticsDto> GetPaymentStatisticsAsync(DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                var queryParams = new List<string> { "page=1", "pageSize=1000" };
                if (fromDate.HasValue)
                    queryParams.Add($"fromDate={Uri.EscapeDataString(fromDate.Value.ToString("yyyy-MM-ddTHH:mm:ss"))}");
                if (toDate.HasValue)
                    queryParams.Add($"toDate={Uri.EscapeDataString(toDate.Value.ToString("yyyy-MM-ddTHH:mm:ss"))}");

                var queryString = "?" + string.Join("&", queryParams);
                var requestUrl = $"/api/Payments/all{queryString}";
                
                _logger.LogInformation($"Calling Payments API: {requestUrl}");
                
                var response = await _appointmentsHttpClient.GetAsync(requestUrl);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning($"Failed to get payment statistics: {response.StatusCode} - {errorContent}");
                    return new PaymentStatisticsDto();
                }

                var jsonContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"Payments API response received. Content length: {jsonContent.Length}");
                
                if (string.IsNullOrWhiteSpace(jsonContent))
                {
                    _logger.LogWarning("Payments API returned empty response");
                    return new PaymentStatisticsDto();
                }

                var result = JsonSerializer.Deserialize<JsonElement>(jsonContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                // Check if the response has Payments property
                if (!result.TryGetProperty("payments", out var paymentsProp))
                {
                    _logger.LogWarning($"Response does not contain 'Payments' property. Available properties: {string.Join(", ", result.EnumerateObject().Select(p => p.Name))}");
                    return new PaymentStatisticsDto();
                }

                var payments = paymentsProp.EnumerateArray().ToList();
                _logger.LogInformation($"Found {payments.Count} payments");
                var stats = new PaymentStatisticsDto
                {
                    TotalCount = payments.Count
                };

                // Calculate statistics
                foreach (var payment in payments)
                {
                    var amount = payment.TryGetProperty("amount", out var amountProp) && amountProp.ValueKind == JsonValueKind.Number
                        ? amountProp.GetInt64()
                        : 0;
                    var status = payment.TryGetProperty("status", out var statusProp) ? statusProp.GetString() ?? "unknown" : "unknown";
                    var vendor = payment.TryGetProperty("vendor", out var vendorProp) ? vendorProp.GetString() ?? "unknown" : "unknown";

                    stats.TotalAmount += amount;

                    // Count by status
                    if (stats.CountByStatus.ContainsKey(status))
                        stats.CountByStatus[status]++;
                    else
                        stats.CountByStatus[status] = 1;

                    // Count by vendor
                    if (stats.CountByVendor.ContainsKey(vendor))
                        stats.CountByVendor[vendor]++;
                    else
                        stats.CountByVendor[vendor] = 1;

                    // Amount by status
                    if (stats.AmountByStatus.ContainsKey(status))
                        stats.AmountByStatus[status] += amount;
                    else
                        stats.AmountByStatus[status] = amount;
                }

                // Generate daily statistics
                stats.DailyStatistics = payments
                    .GroupBy(p => p.TryGetProperty("createdAt", out var createdAtProp) 
                        ? DateTime.Parse(createdAtProp.GetString() ?? DateTime.Now.ToString()).Date 
                        : DateTime.Now.Date)
                    .Select(g => new DailyStatisticsDto
                    {
                        Date = g.Key,
                        Count = g.Count(),
                        Amount = g.Sum(p => p.TryGetProperty("amount", out var amtProp) && amtProp.ValueKind == JsonValueKind.Number
                            ? amtProp.GetInt64()
                            : 0)
                    })
                    .OrderBy(d => d.Date)
                    .ToList();

                _logger.LogInformation($"Payment statistics calculated: Total={stats.TotalCount}, TotalAmount={stats.TotalAmount}");
                
                return stats;
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError(httpEx, "HTTP error getting payment statistics. URL: {Url}", _appointmentsHttpClient.BaseAddress);
                return new PaymentStatisticsDto();
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError(jsonEx, "JSON parsing error getting payment statistics");
                return new PaymentStatisticsDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error getting payment statistics");
                return new PaymentStatisticsDto();
            }
        }

        private async Task<AppointmentStatisticsDto> GetAppointmentStatisticsAsync(DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                var queryParams = new List<string> { "page=1", "pageSize=1000" };
                if (fromDate.HasValue)
                    queryParams.Add($"fromDate={Uri.EscapeDataString(fromDate.Value.ToString("yyyy-MM-ddTHH:mm:ss"))}");
                if (toDate.HasValue)
                    queryParams.Add($"toDate={Uri.EscapeDataString(toDate.Value.ToString("yyyy-MM-ddTHH:mm:ss"))}");

                var queryString = "?" + string.Join("&", queryParams);
                var requestUrl = $"/api/Appointment/all{queryString}";
                
                _logger.LogInformation($"Calling Appointments API: {requestUrl}");
                
                var response = await _appointmentsHttpClient.GetAsync(requestUrl);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning($"Failed to get appointment statistics: {response.StatusCode} - {errorContent}");
                    return new AppointmentStatisticsDto();
                }

                var jsonContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"Appointments API response received. Content length: {jsonContent.Length}");
                
                if (string.IsNullOrWhiteSpace(jsonContent))
                {
                    _logger.LogWarning("Appointments API returned empty response");
                    return new AppointmentStatisticsDto();
                }

                var result = JsonSerializer.Deserialize<JsonElement>(jsonContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                // Check if the response has Appointments property
                if (!result.TryGetProperty("appointments", out var appointmentsProp))
                {
                    _logger.LogWarning($"Response does not contain 'Appointments' property. Available properties: {string.Join(", ", result.EnumerateObject().Select(p => p.Name))}");
                    return new AppointmentStatisticsDto();
                }

                var appointments = appointmentsProp.EnumerateArray().ToList();
                _logger.LogInformation($"Found {appointments.Count} appointments");
                
                var stats = new AppointmentStatisticsDto
                {
                    TotalCount = appointments.Count
                };

                // Calculate statistics
                foreach (var appointment in appointments)
                {
                    var status = appointment.TryGetProperty("status", out var statusProp) && statusProp.ValueKind == JsonValueKind.Number
                        ? statusProp.GetInt32()
                        : 0;

                    var statusName = GetAppointmentStatusName(status);

                    // Count by status
                    if (stats.CountByStatus.ContainsKey(statusName))
                        stats.CountByStatus[statusName]++;
                    else
                        stats.CountByStatus[statusName] = 1;

                    // Count specific statuses
                    switch (status)
                    {
                        case 1: // Confirmed
                            stats.ActiveAppointments++;
                            break;
                        case 3: // Completed
                            stats.CompletedAppointments++;
                            break;
                        case 0: // Pending
                            stats.PendingAppointments++;
                            break;
                        case 4: // PaymentPending
                            stats.PaymentPendingAppointments++;
                            break;
                        case 2: // Cancelled
                            stats.CancelledAppointments++;
                            break;
                    }
                }

                // Generate daily statistics
                stats.DailyStatistics = appointments
                    .GroupBy(a =>
                    {
                        // Try to parse CreateAt from different formats
                        if (a.TryGetProperty("createAt", out var createAtProp))
                        {
                            if (createAtProp.ValueKind == JsonValueKind.String)
                            {
                                var dateStr = createAtProp.GetString();
                                if (!string.IsNullOrEmpty(dateStr) && DateTime.TryParse(dateStr, out var date))
                                    return date.Date;
                            }
                            else if (createAtProp.ValueKind == JsonValueKind.Object)
                            {
                                if (createAtProp.TryGetProperty("Year", out var yearProp)
                                    && createAtProp.TryGetProperty("Month", out var monthProp)
                                    && createAtProp.TryGetProperty("Day", out var dayProp))
                                {
                                    return new DateTime(yearProp.GetInt32(), monthProp.GetInt32(), dayProp.GetInt32());
                                }
                            }
                        }
                        // Fallback: try ScheduledAt
                        if (a.TryGetProperty("scheduledAt", out var scheduledAtProp))
                        {
                            if (scheduledAtProp.ValueKind == JsonValueKind.String)
                            {
                                var dateStr = scheduledAtProp.GetString();
                                if (!string.IsNullOrEmpty(dateStr) && DateTime.TryParse(dateStr, out var date))
                                    return date.Date;
                            }
                        }
                        return DateTime.Now.Date;
                    })
                    .Select(g => new DailyStatisticsDto
                    {
                        Date = g.Key,
                        Count = g.Count()
                    })
                    .OrderBy(d => d.Date)
                    .ToList();

                _logger.LogInformation($"Appointment statistics calculated: Total={stats.TotalCount}, Active={stats.ActiveAppointments}, Completed={stats.CompletedAppointments}, Pending={stats.PendingAppointments}");
                
                return stats;
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError(httpEx, "HTTP error getting appointment statistics. URL: {Url}", _appointmentsHttpClient.BaseAddress);
                return new AppointmentStatisticsDto();
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError(jsonEx, "JSON parsing error getting appointment statistics");
                return new AppointmentStatisticsDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error getting appointment statistics");
                return new AppointmentStatisticsDto();
            }
        }

        private async Task<ReviewStatisticsDto> GetReviewStatisticsAsync(DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                var allReviews = (await _reviewRepository.GetAllAsync()).ToList();

                // Filter by date if provided
                if (fromDate.HasValue || toDate.HasValue)
                {
                    if (fromDate.HasValue)
                        allReviews = allReviews.Where(r => r.CreatedAt >= fromDate.Value).ToList();
                    if (toDate.HasValue)
                        allReviews = allReviews.Where(r => r.CreatedAt <= toDate.Value.AddDays(1)).ToList();
                }

                var stats = new ReviewStatisticsDto
                {
                    TotalCount = allReviews.Count
                };

                if (stats.TotalCount == 0)
                    return stats;

                // Calculate average rating
                stats.AverageRating = allReviews.Average(r => (decimal)r.Rating);

                // Rating distribution (1-5 stars)
                for (int i = 1; i <= 5; i++)
                {
                    stats.RatingDistribution[i] = allReviews.Count(r => (int)r.Rating == i);
                }

                // Generate daily statistics
                stats.DailyStatistics = allReviews
                    .GroupBy(r => r.CreatedAt.Date)
                    .Select(g => new DailyStatisticsDto
                    {
                        Date = g.Key,
                        Count = g.Count()
                    })
                    .OrderBy(d => d.Date)
                    .ToList();

                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting review statistics");
                return new ReviewStatisticsDto();
            }
        }

        private string GetAppointmentStatusName(int status)
        {
            return status switch
            {
                0 => "Pending",
                1 => "Confirmed",
                2 => "Cancelled",
                3 => "Completed",
                4 => "PaymentPending",
                _ => "Unknown"
            };
        }
    }
}

