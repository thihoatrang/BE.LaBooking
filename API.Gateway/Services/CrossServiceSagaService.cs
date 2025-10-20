using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Collections.Generic;
using System.Net.Http.Json;

namespace API.Gateway.Services
{
    public class CrossServiceSagaService
    {
        private readonly HttpClient _httpClient;
        private readonly IServiceDiscoveryService _serviceDiscovery;
        private readonly ILogger<CrossServiceSagaService> _logger;
        private readonly Dictionary<string, object> _sagaStates = new Dictionary<string, object>();

        public CrossServiceSagaService(
            HttpClient httpClient, 
            IServiceDiscoveryService serviceDiscovery,
            ILogger<CrossServiceSagaService> logger)
        {
            _httpClient = httpClient;
            _serviceDiscovery = serviceDiscovery;
            _logger = logger;
        }

        public async Task<object> StartCompleteUserRegistrationSagaAsync(object registrationData)
        {
            var sagaId = Guid.NewGuid().ToString();
            var sagaState = new
            {
                Id = sagaId,
                State = "Started",
                Steps = new List<string>(),
                CreatedAt = DateTime.UtcNow,
                Data = registrationData
            };

            _sagaStates[sagaId] = sagaState;

            try
            {
                _logger.LogInformation($"Starting cross-service saga {sagaId}");

                // Step 1: Create user in Users service
                var usersServiceUrl = await _serviceDiscovery.GetServiceUrlAsync("UsersService");
                var userResponse = await _httpClient.PostAsJsonAsync($"{usersServiceUrl}/api/users", registrationData);
                if (!userResponse.IsSuccessStatusCode)
                {
                    throw new Exception($"Failed to create user: {userResponse.ReasonPhrase}");
                }

                var userJson = await userResponse.Content.ReadAsStringAsync();
                using var userDoc = JsonDocument.Parse(userJson);
                int? createdUserId = null;
                if (userDoc.RootElement.TryGetProperty("result", out var resultEl) &&
                    resultEl.TryGetProperty("id", out var idEl) && idEl.TryGetInt32(out var idVal))
                {
                    createdUserId = idVal;
                }
                _logger.LogInformation($"User created successfully in saga {sagaId}");

                // Step 2: If user is a lawyer, create lawyer profile
                var registrationJson = JsonSerializer.Serialize(registrationData);
                using var regDoc = JsonDocument.Parse(registrationJson);
                var isLawyer = regDoc.RootElement.TryGetProperty("role", out var roleEl) &&
                               string.Equals(roleEl.GetString(), "lawyer", StringComparison.OrdinalIgnoreCase);
                if (isLawyer)
                {
                    var lawyerProfileData = new
                    {
                        UserId = createdUserId,
                        Bio = "New lawyer profile",
                        Spec = "General Practice",
                        LicenseNum = "TBD",
                        ExpYears = 0,
                        Description = "New lawyer",
                        Rating = 0,
                        PricePerHour = 500000,
                        Img = "",
                        DayOfWeek = "Mon,Tue,Wed,Thu,Fri",
                        WorkTime = "09:00-17:00"
                    };

                    var lawyersServiceUrl = await _serviceDiscovery.GetServiceUrlAsync("LawyersService");
                    var lawyerResponse = await _httpClient.PostAsJsonAsync($"{lawyersServiceUrl}/api/lawyers", lawyerProfileData);
                    if (!lawyerResponse.IsSuccessStatusCode)
                    {
                        // Compensate: Delete user
                        if (createdUserId.HasValue)
                        {
                            await _httpClient.DeleteAsync($"{usersServiceUrl}/api/users/{createdUserId.Value}");
                        }
                        throw new Exception($"Failed to create lawyer profile: {lawyerResponse.ReasonPhrase}");
                    }

                    _logger.LogInformation($"Lawyer profile created successfully in saga {sagaId}");
                }

                // Update saga state
                _sagaStates[sagaId] = new
                {
                    Id = sagaId,
                    State = "Completed",
                    Steps = new List<string> { "UserCreated", "LawyerProfileCreated" },
                    CreatedAt = sagaState.CreatedAt,
                    CompletedAt = DateTime.UtcNow,
                    Data = registrationData
                };

                return new
                {
                    SagaId = sagaId,
                    State = "Completed",
                    Message = "Complete user registration saga completed successfully",
                    UserData = userDoc.RootElement
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Cross-service saga {sagaId} failed");
                
                // Update saga state to failed
                _sagaStates[sagaId] = new
                {
                    Id = sagaId,
                    State = "Failed",
                    Steps = new List<string>(),
                    CreatedAt = sagaState.CreatedAt,
                    FailedAt = DateTime.UtcNow,
                    ErrorMessage = ex.Message,
                    Data = registrationData
                };

                throw;
            }
        }

        public async Task<object> StartAppointmentWithUserLawyerSagaAsync(object appointmentData)
        {
            var sagaId = Guid.NewGuid().ToString();
            var sagaState = new
            {
                Id = sagaId,
                State = "Started",
                Steps = new List<string>(),
                CreatedAt = DateTime.UtcNow,
                Data = appointmentData
            };

            _sagaStates[sagaId] = sagaState;

            try
            {
                _logger.LogInformation($"Starting appointment with user-lawyer saga {sagaId}");

                // Step 1: Validate user exists
                var appointmentJson = JsonSerializer.Serialize(appointmentData);
                using var apptDoc = JsonDocument.Parse(appointmentJson);
                int userId = apptDoc.RootElement.TryGetProperty("userId", out var userIdEl) && userIdEl.TryGetInt32(out var uid)
                    ? uid : 0;
                int lawyerId = apptDoc.RootElement.TryGetProperty("lawyerId", out var lawyerIdEl) && lawyerIdEl.TryGetInt32(out var lid)
                    ? lid : 0;

                var usersServiceUrl2 = await _serviceDiscovery.GetServiceUrlAsync("UsersService");
                var userResponse = await _httpClient.GetAsync($"{usersServiceUrl2}/api/users/{userId}");
                if (!userResponse.IsSuccessStatusCode)
                {
                    throw new Exception($"User {userId} not found");
                }

                // Step 2: Validate lawyer exists
                var lawyersServiceUrl2 = await _serviceDiscovery.GetServiceUrlAsync("LawyersService");
                var lawyerResponse = await _httpClient.GetAsync($"{lawyersServiceUrl2}/api/lawyers/GetProfileById/{lawyerId}");
                if (!lawyerResponse.IsSuccessStatusCode)
                {
                    throw new Exception($"Lawyer {lawyerId} not found");
                }

                // Step 3: Create appointment
                var appointmentsServiceUrl = await _serviceDiscovery.GetServiceUrlAsync("AppointmentsService");
                var appointmentResponse = await _httpClient.PostAsJsonAsync($"{appointmentsServiceUrl}/api/appointments/CREATE", appointmentData);
                if (!appointmentResponse.IsSuccessStatusCode)
                {
                    throw new Exception($"Failed to create appointment: {appointmentResponse.ReasonPhrase}");
                }

                var appointmentJsonResp = await appointmentResponse.Content.ReadAsStringAsync();
                using var appointmentDoc = JsonDocument.Parse(appointmentJsonResp);
                _logger.LogInformation($"Appointment created successfully in saga {sagaId}");

                // Update saga state
                _sagaStates[sagaId] = new
                {
                    Id = sagaId,
                    State = "Completed",
                    Steps = new List<string> { "UserValidated", "LawyerValidated", "AppointmentCreated" },
                    CreatedAt = sagaState.CreatedAt,
                    CompletedAt = DateTime.UtcNow,
                    Data = appointmentData
                };

                return new
                {
                    SagaId = sagaId,
                    State = "Completed",
                    Message = "Appointment with user-lawyer saga completed successfully",
                    AppointmentData = appointmentDoc.RootElement
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Appointment with user-lawyer saga {sagaId} failed");
                
                // Update saga state to failed
                _sagaStates[sagaId] = new
                {
                    Id = sagaId,
                    State = "Failed",
                    Steps = new List<string>(),
                    CreatedAt = sagaState.CreatedAt,
                    FailedAt = DateTime.UtcNow,
                    ErrorMessage = ex.Message,
                    Data = appointmentData
                };

                throw;
            }
        }

        public object? GetSagaState(string sagaId)
        {
            return _sagaStates.ContainsKey(sagaId) ? _sagaStates[sagaId] : null;
        }

        public IEnumerable<object> GetAllSagaStates()
        {
            return _sagaStates.Values;
        }
    }
}
