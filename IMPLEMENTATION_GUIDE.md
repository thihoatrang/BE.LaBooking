# Unit Test Implementation Guide

## 📋 Tổng quan

Dự án đã có **62 unit test files** được tạo tự động cho:
- **28 Services**
- **22 Controllers**  
- **12 Repositories**

File thống kê: `Unit_Test_Statistics.xlsx`

## 🚀 Các bước tiếp theo

### 1. Thêm Test Projects vào Solution

Chạy script PowerShell:
```powershell
.\add_tests_to_solution.ps1
```

Hoặc thủ công:
```bash
dotnet sln LawAppointmentApp.sln add Users.Application.Tests/Users.Application.Tests.csproj
dotnet sln LawAppointmentApp.sln add Appointments.Application.Tests/Appointments.Application.Tests.csproj
# ... (lặp lại cho tất cả test projects)
```

### 2. Restore Packages và Build

```bash
# Restore tất cả packages
dotnet restore

# Build solution
dotnet build
```

### 3. Chạy Tests

```bash
# Chạy tất cả tests
dotnet test

# Chạy tests của một project cụ thể
dotnet test Users.Application.Tests

# Chạy tests với code coverage
dotnet test --collect:"XPlat Code Coverage"
```

## 📝 Implement Unit Tests

### Ví dụ: AuthServiceTests.cs

Mở file `Users.Application.Tests/Services/AuthServiceTests.cs` và implement:

```csharp
using Xunit;
using FluentAssertions;
using Moq;
using Users.Application.Services;
using Users.Application.Services.IService;
using Users.Infrastructure.Data;
using Users.Infrastructure.Models;
using Users.Infrastructure.Models.Dtos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Users.Application.Services.Tests
{
    public class AuthServiceTests : IDisposable
    {
        private readonly UserDbContext _context;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            // Setup InMemory database
            var options = new DbContextOptionsBuilder<UserDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            
            _context = new UserDbContext(options);
            _mapperMock = new Mock<IMapper>();
            _configurationMock = new Mock<IConfiguration>();
            _emailServiceMock = new Mock<IEmailService>();
            
            // Setup configuration
            _configurationMock.Setup(x => x["JwtSettings:SecretKey"])
                .Returns("your-super-secret-key-with-at-least-32-characters");
            _configurationMock.Setup(x => x["JwtSettings:Issuer"])
                .Returns("LawAppointmentApp");
            _configurationMock.Setup(x => x["JwtSettings:Audience"])
                .Returns("LawAppointmentApp");
            _configurationMock.Setup(x => x["JwtSettings:ExpiryInMinutes"])
                .Returns("60");
            
            _authService = new AuthService(
                _context,
                _mapperMock.Object,
                _configurationMock.Object,
                _emailServiceMock.Object
            );
        }

        [Fact]
        public async Task LoginAsync_WithValidCredentials_ShouldReturnSuccess()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                Email = "test@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                FullName = "Test User",
                IsActive = true
            };
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            var loginRequest = new LoginRequestDTO
            {
                Email = "test@example.com",
                Password = "password123"
            };

            // Act
            var result = await _authService.LoginAsync(loginRequest);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Token.Should().NotBeNullOrEmpty();
            result.User.Should().NotBeNull();
        }

        [Fact]
        public async Task LoginAsync_WithInvalidEmail_ShouldReturnFailure()
        {
            // Arrange
            var loginRequest = new LoginRequestDTO
            {
                Email = "wrong@example.com",
                Password = "password123"
            };

            // Act
            var result = await _authService.LoginAsync(loginRequest);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Message.Should().Contain("not found");
        }

        [Fact]
        public async Task LoginAsync_WithInvalidPassword_ShouldReturnFailure()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                Email = "test@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                FullName = "Test User",
                IsActive = true
            };
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            var loginRequest = new LoginRequestDTO
            {
                Email = "test@example.com",
                Password = "wrongpassword"
            };

            // Act
            var result = await _authService.LoginAsync(loginRequest);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Message.Should().Contain("Invalid");
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}
```

### Ví dụ: PaymentCalculationServiceTests.cs

```csharp
using Xunit;
using FluentAssertions;
using Moq;
using Appointments.Application.Services;
using Appointments.Infrastructure.Models.Dtos;

namespace Appointments.Application.Services.Tests
{
    public class PaymentCalculationServiceTests
    {
        private readonly Mock<LawyerProfileApiClient> _lawyerApiClientMock;
        private readonly PaymentCalculationService _service;

        public PaymentCalculationServiceTests()
        {
            _lawyerApiClientMock = new Mock<LawyerProfileApiClient>(
                Mock.Of<HttpClient>(),
                Mock.Of<ILogger<LawyerProfileApiClient>>()
            );
            _service = new PaymentCalculationService(_lawyerApiClientMock.Object);
        }

        [Fact]
        public async Task CalculatePaymentAmountAsync_WithValidLawyer_ShouldReturn30PercentDeposit()
        {
            // Arrange
            var lawyerId = 1;
            var pricePerHour = 1000000m; // 1 million VND
            var expectedDeposit = (long)(pricePerHour * 2 * 0.3m); // 600,000 VND

            var lawyerProfile = new
            {
                Id = lawyerId,
                PricePerHour = pricePerHour
            };

            _lawyerApiClientMock
                .Setup(x => x.GetLawyerProfileByIdAsync(lawyerId))
                .ReturnsAsync(lawyerProfile);

            // Act
            var result = await _service.CalculatePaymentAmountAsync(lawyerId);

            // Assert
            result.Should().Be(expectedDeposit);
        }

        [Fact]
        public async Task CalculatePaymentAmountAsync_WithNonExistentLawyer_ShouldThrowException()
        {
            // Arrange
            var lawyerId = 999;

            _lawyerApiClientMock
                .Setup(x => x.GetLawyerProfileByIdAsync(lawyerId))
                .ReturnsAsync((object?)null);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => _service.CalculatePaymentAmountAsync(lawyerId)
            );
        }
    }
}
```

## 🎯 Best Practices

### 1. Test Naming Convention
- Format: `MethodName_Scenario_ExpectedBehavior`
- Ví dụ: `LoginAsync_WithValidCredentials_ShouldReturnSuccess`

### 2. AAA Pattern (Arrange-Act-Assert)
```csharp
[Fact]
public void TestMethod()
{
    // Arrange - Setup test data
    
    // Act - Execute the method being tested
    
    // Assert - Verify the results
}
```

### 3. Use FluentAssertions
```csharp
// Instead of:
Assert.Equal(expected, actual);

// Use:
actual.Should().Be(expected);
result.IsSuccess.Should().BeTrue();
collection.Should().HaveCount(3);
```

### 4. Mock Dependencies
```csharp
var mockRepo = new Mock<IRepository>();
mockRepo.Setup(x => x.GetByIdAsync(1))
    .ReturnsAsync(new Entity { Id = 1 });
```

### 5. Use InMemory Database for Integration Tests
```csharp
var options = new DbContextOptionsBuilder<DbContext>()
    .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
    .Options;
```

## 📊 Track Progress trong Excel

1. Mở `Unit_Test_Statistics.xlsx`
2. Update cột "Status" cho mỗi test file:
   - `Pending` - Chưa implement
   - `In Progress` - Đang implement
   - `Completed` - Đã implement đầy đủ
3. Update cột "Test Count" với số lượng tests thực tế

## 🔍 Checklist cho mỗi Service/Controller/Repository

- [ ] Test successful cases (Happy Path)
- [ ] Test error cases (Exception handling)
- [ ] Test edge cases (Boundary conditions)
- [ ] Test null/empty inputs
- [ ] Test with mocked dependencies
- [ ] Test async methods properly
- [ ] Achieve >80% code coverage

## 📦 Required NuGet Packages

Tất cả test projects đã có các packages cần thiết:
- `xunit` - Test framework
- `Moq` - Mocking framework
- `FluentAssertions` - Assertion library
- `Microsoft.EntityFrameworkCore.InMemory` - In-memory database
- `AutoMapper` - Mapping tests

## 🚨 Common Issues và Solutions

### Issue 1: Tests không tìm thấy dependencies
**Solution**: Đảm bảo đã add project references vào .csproj file

### Issue 2: Mock không hoạt động
**Solution**: Đảm bảo interface được mock đúng, không mock concrete class

### Issue 3: Database context conflicts
**Solution**: Sử dụng InMemory database với unique database name

## 📈 Code Coverage Goal

Mục tiêu: **>80% code coverage** cho tất cả projects

Kiểm tra coverage:
```bash
dotnet test --collect:"XPlat Code Coverage"
```

## ✅ Next Steps

1. ✅ Test projects đã được tạo
2. ✅ Test files skeleton đã được tạo
3. ⏭️ **Thêm test projects vào solution** (chạy script PowerShell)
4. ⏭️ **Implement tests cho từng component**
5. ⏭️ **Chạy tests và fix issues**
6. ⏭️ **Update Excel với progress**

Bắt đầu với các services quan trọng nhất:
- `AuthService`
- `PaymentCalculationService`
- `AppointmentService`
- `TransactionService`

