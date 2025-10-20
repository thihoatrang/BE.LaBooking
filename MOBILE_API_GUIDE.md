# 📱 Mobile App API Integration Guide

## Tổng quan

Backend đã được tối ưu hóa cho mobile app với API Gateway và Saga Pattern. Tất cả APIs đều có thể sử dụng từ mobile app.

## 🔗 Base URL

```
Production: https://your-domain.com
Development: https://localhost:5000
```

## 🔐 Authentication

### Login
```http
POST /api/mobile/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "password123"
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "isSuccess": true,
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "user": {
      "id": 1,
      "email": "user@example.com",
      "fullName": "John Doe",
      "role": "customer"
    }
  },
  "message": "Mobile login successful"
}
```

### Register
```http
POST /api/mobile/register
Content-Type: application/json

{
  "email": "newuser@example.com",
  "password": "password123",
  "fullName": "Jane Doe",
  "phoneNumber": "0123456789",
  "role": "customer"
}
```

## 👥 User Management

### Get User Profile
```http
GET /api/users/{userId}
Authorization: Bearer {token}
```

### Update User Profile
```http
PUT /api/users/{userId}
Authorization: Bearer {token}
Content-Type: application/json

{
  "fullName": "Updated Name",
  "phoneNumber": "0987654321"
}
```

## ⚖️ Lawyer Services

### Get All Lawyers
```http
GET /api/mobile/lawyers
```

**Response:**
```json
{
  "success": true,
  "data": {
    "isSuccess": true,
    "result": [
      {
        "id": 1,
        "userId": 1,
        "bio": "Experienced lawyer",
        "spec": "Criminal Law",
        "rating": 4.8,
        "pricePerHour": 500000
      }
    ]
  },
  "message": "Lawyers retrieved successfully"
}
```

### Get Lawyer by ID
```http
GET /api/lawyers/GetProfileById/{lawyerId}
```

## 📅 Appointment Management

### Create Appointment
```http
POST /api/mobile/appointment
Authorization: Bearer {token}
Content-Type: application/json

{
  "userId": 1,
  "lawyerId": 1,
  "scheduledAt": "2024-01-15T10:00:00Z",
  "slot": "Morning",
  "spec": "Criminal Law",
  "services": ["Consultation"],
  "note": "Urgent case"
}
```

### Get User Appointments
```http
GET /api/mobile/user/{userId}/appointments
Authorization: Bearer {token}
```

### Update Appointment Status
```http
PUT /api/appointments/{id}/confirm
Authorization: Bearer {token}
```

```http
PUT /api/appointments/{id}/cancel
Authorization: Bearer {token}
```

## 🔄 Saga Pattern Endpoints

### Check Saga State
```http
GET /api/saga/state/{sagaId}
```

### Get All Saga States
```http
GET /api/saga/all-states
```

## 🏥 Health Check

### API Health
```http
GET /api/mobile/health
```

**Response:**
```json
{
  "success": true,
  "message": "Mobile API Gateway is healthy",
  "timestamp": "2024-01-15T10:00:00Z",
  "services": {
    "users": "https://localhost:7000",
    "lawyers": "https://localhost:7110",
    "appointments": "https://localhost:7001"
  }
}
```

## 📱 Mobile App Integration

### 1. Flutter/Dart
```dart
class ApiService {
  static const String baseUrl = 'https://localhost:5000';
  
  Future<Map<String, dynamic>> login(String email, String password) async {
    final response = await http.post(
      Uri.parse('$baseUrl/api/mobile/login'),
      headers: {'Content-Type': 'application/json'},
      body: jsonEncode({
        'email': email,
        'password': password,
      }),
    );
    
    return jsonDecode(response.body);
  }
}
```

### 2. React Native/JavaScript
```javascript
const API_BASE_URL = 'https://localhost:5000';

export const loginUser = async (email, password) => {
  const response = await fetch(`${API_BASE_URL}/api/mobile/login`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({ email, password }),
  });
  
  return await response.json();
};
```

### 3. Native iOS (Swift)
```swift
class ApiService {
    static let baseURL = "https://localhost:5000"
    
    func login(email: String, password: String) async throws -> [String: Any] {
        let url = URL(string: "\(Self.baseURL)/api/mobile/login")!
        var request = URLRequest(url: url)
        request.httpMethod = "POST"
        request.setValue("application/json", forHTTPHeaderField: "Content-Type")
        
        let body = ["email": email, "password": password]
        request.httpBody = try JSONSerialization.data(withJSONObject: body)
        
        let (data, _) = try await URLSession.shared.data(for: request)
        return try JSONSerialization.jsonObject(with: data) as! [String: Any]
    }
}
```

### 4. Native Android (Kotlin)
```kotlin
class ApiService {
    companion object {
        private const val BASE_URL = "https://localhost:5000"
    }
    
    suspend fun login(email: String, password: String): Map<String, Any> {
        val client = OkHttpClient()
        val json = JSONObject().apply {
            put("email", email)
            put("password", password)
        }
        
        val request = Request.Builder()
            .url("$BASE_URL/api/mobile/login")
            .post(json.toString().toRequestBody("application/json".toMediaType()))
            .build()
            
        val response = client.newCall(request).execute()
        return JSONObject(response.body?.string() ?: "").toMap()
    }
}
```

## 🔒 Security Best Practices

### 1. Token Storage
- **iOS**: Keychain
- **Android**: Encrypted SharedPreferences
- **Flutter**: flutter_secure_storage
- **React Native**: react-native-keychain

### 2. Certificate Pinning
```dart
// Flutter example
class ApiClient {
  static final HttpClient _client = HttpClient()
    ..badCertificateCallback = (cert, host, port) {
      // Implement certificate pinning
      return _isValidCertificate(cert);
    };
}
```

### 3. Request Timeout
```javascript
// React Native example
const apiCall = async (url, options) => {
  const controller = new AbortController();
  const timeoutId = setTimeout(() => controller.abort(), 10000); // 10s timeout
  
  try {
    const response = await fetch(url, {
      ...options,
      signal: controller.signal,
    });
    clearTimeout(timeoutId);
    return response;
  } catch (error) {
    clearTimeout(timeoutId);
    throw error;
  }
};
```

## 🚀 Performance Tips

### 1. Caching
- Cache lawyer profiles
- Cache user appointments
- Implement offline support

### 2. Pagination
```http
GET /api/lawyers?page=1&limit=20
```

### 3. Image Optimization
- Use WebP format
- Implement lazy loading
- Cache images locally

## 📊 Error Handling

### Standard Error Response
```json
{
  "success": false,
  "message": "Error description",
  "details": "Detailed error information",
  "code": "ERROR_CODE"
}
```

### Common Error Codes
- `AUTH_REQUIRED`: Authentication required
- `INVALID_TOKEN`: Token expired or invalid
- `USER_NOT_FOUND`: User not found
- `APPOINTMENT_CONFLICT`: Appointment time conflict
- `SAGA_FAILED`: Saga transaction failed

## 🔄 Offline Support

### 1. Queue Failed Requests
```dart
class OfflineQueue {
  static final List<Map<String, dynamic>> _queue = [];
  
  static void addToQueue(String endpoint, Map<String, dynamic> data) {
    _queue.add({
      'endpoint': endpoint,
      'data': data,
      'timestamp': DateTime.now().millisecondsSinceEpoch,
    });
  }
  
  static Future<void> processQueue() async {
    if (await _isOnline()) {
      for (final item in _queue) {
        await _retryRequest(item);
      }
      _queue.clear();
    }
  }
}
```

### 2. Local Storage
- Store user data locally
- Cache appointment data
- Save draft appointments

## 📈 Analytics & Monitoring

### 1. Track API Usage
```javascript
const trackApiCall = (endpoint, duration, success) => {
  analytics.track('api_call', {
    endpoint,
    duration,
    success,
    timestamp: new Date().toISOString(),
  });
};
```

### 2. Error Reporting
```dart
void reportError(dynamic error, StackTrace stackTrace) {
  FirebaseCrashlytics.instance.recordError(error, stackTrace);
}
```

## 🎯 Testing

### 1. Unit Tests
```dart
test('should login user successfully', () async {
  final result = await apiService.login('test@example.com', 'password');
  expect(result['success'], true);
  expect(result['data']['token'], isNotNull);
});
```

### 2. Integration Tests
```javascript
describe('Appointment API', () => {
  it('should create appointment successfully', async () => {
    const appointment = {
      userId: 1,
      lawyerId: 1,
      scheduledAt: '2024-01-15T10:00:00Z',
      slot: 'Morning',
    };
    
    const response = await createAppointment(appointment);
    expect(response.success).toBe(true);
  });
});
```

## 🚀 Deployment

### 1. Environment Configuration
```dart
class Environment {
  static const String dev = 'https://localhost:5000';
  static const String staging = 'https://staging-api.yourdomain.com';
  static const String prod = 'https://api.yourdomain.com';
  
  static String get baseUrl {
    switch (kDebugMode) {
      case true: return dev;
      default: return prod;
    }
  }
}
```

### 2. CI/CD Integration
```yaml
# GitHub Actions example
- name: Run API Tests
  run: |
    flutter test test/api/
    npm test -- --testPathPattern=api
```

---

## ✅ **Kết luận**

Backend của bạn **ĐÃ SẴN SÀNG** cho mobile app với:

- ✅ **Complete API endpoints** cho tất cả features
- ✅ **JWT Authentication** system
- ✅ **CORS configuration** cho mobile
- ✅ **Mobile-specific endpoints** với `/api/mobile/`
- ✅ **Saga Pattern** đảm bảo data consistency
- ✅ **Error handling** và response format chuẩn
- ✅ **Health check** endpoints
- ✅ **Cross-service coordination**

Bạn có thể bắt đầu phát triển mobile app ngay bây giờ! 🎉
