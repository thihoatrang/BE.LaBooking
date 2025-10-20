# Saga Pattern và API Gateway Implementation - Complete Backend

## Tổng quan

Toàn bộ project Backend đã được chuyển đổi hoàn toàn sang sử dụng **Saga Pattern** cho quản lý giao dịch phân tán và **API Gateway** với Ocelot để điều phối tất cả các microservices.

## Cấu trúc mới

### 1. API Gateway (API.Gateway)
- **Vị trí**: `API.Gateway/`
- **Port**: 5000 (HTTPS)
- **Chức năng**: Điều phối tất cả các request đến các microservices
- **Cấu hình**: `ocelot.json` định nghĩa routing rules

### 2. Saga Pattern Implementation

#### Appointments Service Saga
- **Vị trí**: `Appointments.Services.API/Services/Saga/`
- **Chức năng**: Quản lý giao dịch phân tán cho việc tạo appointment
- **Các trạng thái**: Started → WorkSlotDeactivated → EmailSent → Completed

#### Users Service Saga
- **Vị trí**: `Users.Services.API/Services/Saga/`
- **Chức năng**: Quản lý giao dịch phân tán cho user registration và management
- **Các trạng thái**: Started → UserCreated → EmailSent → Completed

#### Lawyers Service Saga
- **Vị trí**: `LA.Services.API/Services/Saga/`
- **Chức năng**: Quản lý giao dịch phân tán cho lawyer profile và work slots
- **Các trạng thái**: Started → ProfileCreated → WorkSlotsCreated → Completed

#### Cross-Service Saga
- **Vị trí**: `API.Gateway/Services/`
- **Chức năng**: Quản lý giao dịch phức tạp liên quan đến nhiều services
- **Các loại**: Complete User Registration, Appointment with User-Lawyer

## Cách sử dụng

### 1. Chạy API Gateway
```bash
cd API.Gateway
dotnet run
```

### 2. Chạy các microservices
```bash
# Users Service (Port 7000)
cd Users.Services.API
dotnet run

# Lawyers Service (Port 7110)  
cd LA.Services.API
dotnet run

# Appointments Service (Port 7001)
cd Appointments.Services.API
dotnet run
```

### 3. Test API thông qua Gateway

#### User Registration với Saga Pattern
```http
POST https://localhost:5000/api/auth/register
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "password123",
  "fullName": "John Doe",
  "phoneNumber": "0123456789",
  "role": "customer"
}
```

#### Complete User Registration (Cross-Service Saga)
```http
POST https://localhost:5000/api/saga/complete-user-registration
Content-Type: application/json

{
  "email": "lawyer@example.com",
  "password": "password123",
  "fullName": "Jane Lawyer",
  "phoneNumber": "0987654321",
  "role": "lawyer"
}
```

#### Tạo appointment mới (sử dụng Saga Pattern)
```http
POST https://localhost:5000/api/appointments/CREATE
Content-Type: application/json

{
  "userId": 1,
  "lawyerId": 1,
  "scheduledAt": "2024-01-15T10:00:00Z",
  "slot": "Morning",
  "spec": "Criminal Law",
  "services": ["Consultation", "Document Review"],
  "note": "Urgent case"
}
```

#### Appointment với User-Lawyer Validation (Cross-Service Saga)
```http
POST https://localhost:5000/api/saga/appointment-with-user-lawyer
Content-Type: application/json

{
  "userId": 1,
  "lawyerId": 1,
  "scheduledAt": "2024-01-15T10:00:00Z",
  "slot": "Morning",
  "spec": "Criminal Law",
  "services": ["Consultation"],
  "note": "Validated appointment"
}
```

#### Kiểm tra trạng thái Saga
```http
# Individual service saga
GET https://localhost:5000/api/appointments/{id}/saga-state
GET https://localhost:5000/api/lawyers/{id}/saga-state

# Cross-service saga
GET https://localhost:5000/api/saga/state/{sagaId}
GET https://localhost:5000/api/saga/all-states
```

#### Hủy appointment (với compensation)
```http
PUT https://localhost:5000/api/appointments/{id}/cancel
```

## Saga Pattern Flow

### 1. User Registration Saga
1. **Started**: Bắt đầu saga
2. **UserCreated**: Tạo user record
3. **EmailSent**: Gửi email chào mừng
4. **Completed**: Hoàn thành saga thành công

### 2. Lawyer Profile Saga
1. **Started**: Bắt đầu saga
2. **ProfileCreated**: Tạo lawyer profile
3. **WorkSlotsCreated**: Tạo work slots
4. **Completed**: Hoàn thành saga thành công

### 3. Appointment Saga
1. **Started**: Bắt đầu saga, tạo appointment record
2. **WorkSlotDeactivated**: Deactivate work slot của lawyer
3. **EmailSent**: Gửi email thông báo cho lawyer
4. **Completed**: Hoàn thành saga thành công

### 4. Cross-Service Saga
1. **Started**: Bắt đầu cross-service saga
2. **UserValidated**: Validate user exists
3. **LawyerValidated**: Validate lawyer exists
4. **AppointmentCreated**: Tạo appointment
5. **Completed**: Hoàn thành saga thành công

### Compensation (Khi có lỗi)
1. **Compensating**: Bắt đầu quá trình bù trừ
2. **Rollback Operations**: Hoàn tác các thao tác đã thực hiện
3. **Cleanup Resources**: Dọn dẹp tài nguyên
4. **Failed**: Đánh dấu saga thất bại

## Lợi ích

### Saga Pattern
- ✅ **Quản lý giao dịch phân tán**: Xử lý các giao dịch phức tạp qua nhiều services
- ✅ **Tự động compensation**: Tự động hoàn tác khi có lỗi xảy ra
- ✅ **Đảm bảo tính nhất quán dữ liệu**: Duy trì tính toàn vẹn dữ liệu
- ✅ **Theo dõi trạng thái giao dịch**: Monitor và debug dễ dàng
- ✅ **Cross-Service Coordination**: Điều phối các services khác nhau
- ✅ **Fault Tolerance**: Xử lý lỗi một cách graceful

### API Gateway
- ✅ **Điểm truy cập duy nhất**: Tất cả requests đều qua Gateway
- ✅ **Load balancing và routing**: Phân phối tải và định tuyến thông minh
- ✅ **Centralized management**: Quản lý tập trung tất cả APIs
- ✅ **Cross-Service Saga**: Hỗ trợ giao dịch phức tạp liên service
- ✅ **Monitoring và Logging**: Theo dõi và ghi log tập trung
- ✅ **Security**: Bảo mật và authentication tập trung

## Cấu hình Ports

| Service | Port | URL |
|---------|------|-----|
| API Gateway | 5000 | https://localhost:5000 |
| Users Service | 7000 | https://localhost:7000 |
| Lawyers Service | 7110 | https://localhost:7110 |
| Appointments Service | 7001 | https://localhost:7001 |

## Monitoring và Debugging

### Logs
- Tất cả các service đều có logging chi tiết
- Saga states được track và log
- API Gateway logs tất cả requests

### Health Checks
- Mỗi service có endpoint health check
- API Gateway có thể monitor health của downstream services

## Lưu ý quan trọng

1. **Thứ tự khởi động**: Khởi động các microservices trước, sau đó mới khởi động API Gateway
2. **Database**: Đảm bảo tất cả databases đã được setup và migrate
3. **CORS**: Đã cấu hình CORS cho frontend (localhost:5173)
4. **HTTPS**: Tất cả services đều sử dụng HTTPS trong development

## Troubleshooting

### Lỗi thường gặp
1. **Port conflicts**: Kiểm tra các port đã được sử dụng
2. **Database connection**: Kiểm tra connection strings
3. **Service discovery**: Đảm bảo các services có thể giao tiếp với nhau
4. **Saga timeout**: Cấu hình timeout cho các saga operations
