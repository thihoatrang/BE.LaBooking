using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace Chat.Application.Services;

public class KnowledgeService : IKnowledgeService
{
    private readonly IVectorStoreService _vectorStore;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILegalDocumentService _legalDocumentService;

    public KnowledgeService(IVectorStoreService vectorStore, HttpClient httpClient, IConfiguration configuration, ILegalDocumentService legalDocumentService)
    {
        _vectorStore = vectorStore;
        _httpClient = httpClient;
        _configuration = configuration;
        _legalDocumentService = legalDocumentService;
    }

    public async Task IndexLawyersDataAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // 1. Lấy danh sách luật sư
            var lawyersApi = _configuration["ServiceUrls:LawyersAPI"] ?? "https://localhost:7110";
            var lawyersResponse = await _httpClient.GetStringAsync($"{lawyersApi}/api/Lawyer/GetAllLawyerProfile", cancellationToken);

            var root = JsonSerializer.Deserialize<JsonElement>(lawyersResponse);
            if (!root.TryGetProperty("result", out var lawyersArray) || lawyersArray.ValueKind != JsonValueKind.Array)
            {
                Console.WriteLine("[IndexLawyers] Invalid response format from Lawyers API");
                return;
            }

            Console.WriteLine($"[IndexLawyers] Found {lawyersArray.GetArrayLength()} lawyers");

            // 2. Lấy danh sách user IDs để gọi Users API
            var userIds = lawyersArray.EnumerateArray()
                .Select(l => l.GetProperty("userId").GetInt32())
                .ToList();

            // 3. Gọi Users API để lấy thông tin tên (fullName nằm trong result.user.fullName)
            var usersApi = _configuration["ServiceUrls:UsersAPI"] ?? "https://localhost:7071";
            var usersDict = new Dictionary<int, string>(); // userId -> name

            foreach (var userId in userIds)
            {
                try
                {
                    var userResponse = await _httpClient.GetStringAsync($"{usersApi}/api/UserWithLawyerProfile/{userId}", cancellationToken);
                    var userRoot = JsonSerializer.Deserialize<JsonElement>(userResponse);

                    string name = $"User{userId}";

                    // Duyệt vào result → user → fullName
                    if (userRoot.TryGetProperty("result", out var result) &&
                        result.TryGetProperty("user", out var userObj) &&
                        userObj.TryGetProperty("fullName", out var nameProp))
                    {
                        var fullName = nameProp.GetString();
                        if (!string.IsNullOrWhiteSpace(fullName))
                        {
                            name = fullName;
                        }
                    }

                    usersDict[userId] = name;
                    Console.WriteLine($"[IndexLawyers] Mapped user {userId} -> {name}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[IndexLawyers] Failed to get user {userId}: {ex.Message}");
                    usersDict[userId] = $"User{userId}";
                }
            }

            // 4. Index từng luật sư với tên đầy đủ
            foreach (var lawyer in lawyersArray.EnumerateArray())
            {
                var id = lawyer.GetProperty("id").GetInt32();
                var userId = lawyer.GetProperty("userId").GetInt32();
                var bio = lawyer.GetProperty("bio").GetString() ?? "";
                var spec = lawyer.TryGetProperty("spec", out var specProp)
                    ? string.Join(", ", specProp.EnumerateArray().Select(s => s.GetString()))
                    : "";
                var expYears = lawyer.GetProperty("expYears").GetInt32();
                var rating = lawyer.GetProperty("rating").GetDouble();
                var price = lawyer.GetProperty("pricePerHour").GetDouble();
                var location = lawyer.GetProperty("description").GetString() ?? "";

                var name = usersDict.GetValueOrDefault(userId, $"User{userId}");

                // Tạo nội dung đầy đủ — có \n để xuống dòng
                var content = $"Luật sư: {name}\n" +
                              $"Chuyên môn: {spec}\n" +
                              $"Kinh nghiệm: {expYears} năm\n" +
                              $"Đánh giá: {rating:F1}/5\n" +
                              $"Giá: {price:C0}/giờ\n" +
                              $"Vị trí: {location}\n" +
                              $"Giới thiệu: {bio}\n";

                var metadata = new Dictionary<string, object>
                {
                    ["type"] = "lawyer",
                    ["id"] = id,
                    ["user_id"] = userId,
                    ["fullName"] = name,
                    ["specialization"] = spec,
                    ["experience"] = expYears,
                    ["rating"] = (float)rating,
                    ["price"] = (float)price
                };

                await _vectorStore.IndexDocumentAsync($"lawyer_{id}", content, metadata, cancellationToken);
                Console.WriteLine($"[IndexLawyers] Indexed: {name} (ID: {id})");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] IndexLawyersDataAsync failed: {ex}");
        }
    }

    public async Task IndexUsersDataAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var usersApi = _configuration["ServiceUrls:UsersAPI"] ?? "https://localhost:7071/";
            var response = await _httpClient.GetStringAsync($"{usersApi}/api/UserWithLawyerProfile", cancellationToken);
            var users = JsonSerializer.Deserialize<JsonElement>(response);

            if (users.ValueKind == JsonValueKind.Array)
            {
                foreach (var user in users.EnumerateArray())
                {
                    var id = user.GetProperty("id").GetString() ?? Guid.NewGuid().ToString();
                    var name = user.GetProperty("name").GetString() ?? "";
                    var email = user.GetProperty("email").GetString() ?? "";
                    var phone = user.TryGetProperty("phone", out var phoneProp) ? phoneProp.GetString() ?? "" : "";

                    var content = $"Khách hàng: {name}\nEmail: {email}\nSố điện thoại: {phone}";
                    
                    var metadata = new Dictionary<string, object>
                    {
                        ["type"] = "user",
                        ["user_id"] = id,
                        ["name"] = name,
                        ["email"] = email,
                        ["phone"] = phone
                    };

                    await _vectorStore.IndexDocumentAsync($"user_{id}", content, metadata, cancellationToken);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error indexing users data: {ex.Message}");
        }
    }

    public async Task IndexAppointmentsDataAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var appointmentsApi = _configuration["ServiceUrls:AppointmentsAPI"] ?? "https://localhost:7073/";
            var response = await _httpClient.GetStringAsync($"{appointmentsApi}/api/appointments", cancellationToken);
            var appointments = JsonSerializer.Deserialize<JsonElement>(response);

            if (appointments.ValueKind == JsonValueKind.Array)
            {
                foreach (var appointment in appointments.EnumerateArray())
                {
                    var id = appointment.GetProperty("id").GetString() ?? Guid.NewGuid().ToString();
                    var date = appointment.TryGetProperty("appointmentDate", out var dateProp) ? dateProp.GetString() ?? "" : "";
                    var status = appointment.TryGetProperty("status", out var statusProp) ? statusProp.GetString() ?? "" : "";
                    var notes = appointment.TryGetProperty("notes", out var notesProp) ? notesProp.GetString() ?? "" : "";

                    var content = $"Lịch hẹn: {id}\nNgày: {date}\nTrạng thái: {status}\nGhi chú: {notes}";
                    
                    var metadata = new Dictionary<string, object>
                    {
                        ["type"] = "appointment",
                        ["appointment_id"] = id,
                        ["date"] = date,
                        ["status"] = status,
                        ["notes"] = notes
                    };

                    await _vectorStore.IndexDocumentAsync($"appointment_{id}", content, metadata, cancellationToken);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error indexing appointments data: {ex.Message}");
        }
    }

    public async Task IndexServicesAndPracticeAreasAsync(CancellationToken cancellationToken = default)
    {
    try
    {
        // 1. Lấy danh sách Service (Dịch vụ)
        var servicesApi = _configuration["ServiceUrls:CoreAPI"] ?? "https://localhost:7073";
        var servicesResponse = await _httpClient.GetStringAsync($"{servicesApi}/api/Service", cancellationToken);
        var services = JsonSerializer.Deserialize<JsonElement>(servicesResponse);

        if (services.ValueKind == JsonValueKind.Array)
        {
            foreach (var service in services.EnumerateArray())
            {
                var id = service.GetProperty("id").GetInt32();
                var name = service.GetProperty("name").GetString() ?? "";
                var description = service.GetProperty("description").GetString() ?? "";
                var price = service.TryGetProperty("price", out var priceProp) ? priceProp.GetDouble() : 0;

                var content = $"Dịch vụ pháp lý: {name}\n" +
                              $"Mô tả: {description}\n" +
                              $"Mức phí: {price:C0}\n" +
                              $"Liên hệ để được tư vấn chi tiết về dịch vụ này.";

                var metadata = new Dictionary<string, object>
                {
                    ["type"] = "service",
                    ["service_id"] = id,
                    ["name"] = name,
                    ["description"] = description,
                    ["price"] = (float)price
                };

                await _vectorStore.IndexDocumentAsync($"service_{id}", content, metadata, cancellationToken);
            }
        }

        // 2. Lấy danh sách PracticeArea (Lĩnh vực hành nghề)
        var practiceAreasResponse = await _httpClient.GetStringAsync($"{servicesApi}/api/practice-areas", cancellationToken);
        var practiceAreas = JsonSerializer.Deserialize<JsonElement>(practiceAreasResponse);

        if (practiceAreas.ValueKind == JsonValueKind.Array)
        {
            foreach (var area in practiceAreas.EnumerateArray())
            {
                var id = area.GetProperty("id").GetInt32();
                var name = area.GetProperty("name").GetString() ?? "";
                var description = area.GetProperty("description").GetString() ?? "";
                var category = area.TryGetProperty("category", out var catProp) ? catProp.GetString() ?? "" : "";

                var content = $"Lĩnh vực pháp lý: {name}\n" +
                              $"Thuộc nhóm: {category}\n" +
                              $"Mô tả: {description}\n" +
                              $"Các luật sư chuyên về lĩnh vực này có thể tư vấn chi tiết.";

                var metadata = new Dictionary<string, object>
                {
                    ["type"] = "practice_area",
                    ["area_id"] = id,
                    ["name"] = name,
                    ["category"] = category,
                    ["description"] = description
                };

                await _vectorStore.IndexDocumentAsync($"practice_area_{id}", content, metadata, cancellationToken);
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[ERROR] IndexServicesAndPracticeAreasAsync failed: {ex}");
    }
    }

    public async Task IndexLegalDocumentsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _legalDocumentService.IndexLegalDocumentsAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] IndexLegalDocumentsAsync failed: {ex}");
        }
    }


    public async Task IndexCustomDataAsync(string content, string source, Dictionary<string, object>? metadata = null, CancellationToken cancellationToken = default)
    {
        var id = $"custom_{Guid.NewGuid()}";
        var finalMetadata = metadata ?? new Dictionary<string, object>();
        finalMetadata["type"] = "custom";
        finalMetadata["source"] = source;
        
        await _vectorStore.IndexDocumentAsync(id, content, finalMetadata, cancellationToken);
    }

    public async Task RebuildKnowledgeBaseAsync(CancellationToken cancellationToken = default)
    {
        // Clear existing data
        await _vectorStore.ClearAllAsync(cancellationToken);
        
        // Re-index all data
        await IndexLawyersDataAsync(cancellationToken);
        await IndexUsersDataAsync(cancellationToken);
        await IndexAppointmentsDataAsync(cancellationToken);
        await IndexServicesAndPracticeAreasAsync(cancellationToken);
        await IndexLegalDocumentsAsync(cancellationToken);
        
        // Add some general legal knowledge
        await IndexCustomDataAsync(
            "Dịch vụ tư vấn pháp luật bao gồm: tư vấn dân sự, hình sự, thương mại, lao động, đất đai, hôn nhân gia đình. Luật sư có thể hỗ trợ soạn thảo hợp đồng, đại diện tại tòa án, tư vấn pháp lý trực tiếp.",
            "general_legal_info",
            new Dictionary<string, object> { ["category"] = "legal_services" },
            cancellationToken
        );
        
        await IndexCustomDataAsync(
            "Quy trình đặt lịch tư vấn: 1) Chọn luật sư phù hợp 2) Chọn thời gian rảnh 3) Điền thông tin liên hệ 4) Mô tả vấn đề pháp lý 5) Xác nhận lịch hẹn. Phí tư vấn sẽ được thông báo trước khi xác nhận.",
            "booking_process",
            new Dictionary<string, object> { ["category"] = "booking" },
            cancellationToken
        );
    }
}
