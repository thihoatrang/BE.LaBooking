# Payment API Documentation

## Overview
API thanh toán cuộc hẹn tương tự như code Spring Boot tham khảo, hỗ trợ VNPay payment gateway.

## Endpoints

### 1. Tạo URL thanh toán cho cuộc hẹn
**POST** `/api/payments/create-url-for-appointment`

Tạo URL thanh toán VNPay cho cuộc hẹn với luật sư.

**Request Body:**
```json
{
  "vendor": "vnpay",
  "orderId": "optional-order-id", // Nếu không có sẽ tự động tạo
  "lawyerId": 1,
  "durationHours": 2,
  "orderInfo": "Thanh toan cuoc hen voi luat su",
  "returnUrl": "https://localhost:5173/payment-success", // Optional
  "ipnUrl": "https://localhost:7073/api/payments/ipn?vendor=vnpay" // Optional
}
```

**Response:**
```json
"https://sandbox.vnpayment.vn/paymentv2/vpcpay.html?vnp_Version=2.1.0&vnp_Command=pay&..."
```

### 2. Xử lý callback từ VNPay
**GET** `/api/payments/return`

Endpoint để VNPay redirect về sau khi thanh toán.

**Query Parameters:**
- Tất cả parameters từ VNPay callback

**Response:**
```json
{
  "orderId": "order-id",
  "vendor": "vnpay",
  "status": "success",
  "transactionId": "vnpay-transaction-id",
  "message": "Payment successful"
}
```

### 3. Xử lý IPN từ VNPay
**GET** `/api/payments/ipn`

Endpoint để VNPay gửi thông báo thanh toán.

**Response:**
```
OK
```

### 4. Kiểm tra trạng thái thanh toán
**GET** `/api/payments/status/{orderId}`

Kiểm tra trạng thái thanh toán theo Order ID.

**Response:**
```json
{
  "orderId": "order-id",
  "vendor": "vnpay",
  "status": "success",
  "transactionId": "vnpay-transaction-id",
  "message": "Payment successful"
}
```

### 5. Lấy thông tin cuộc hẹn theo Order ID
**GET** `/api/payments/appointment/{orderId}`

Lấy thông tin cuộc hẹn sau khi thanh toán thành công.

**Response:**
```json
{
  "orderId": "order-id",
  "amount": 500000,
  "status": "success",
  "transactionId": "vnpay-transaction-id",
  "createdAt": "2024-01-01T10:00:00Z",
  "message": "Payment successful"
}
```

## Configuration

Cấu hình VNPay trong `appsettings.json`:

```json
{
  "Payments": {
    "VnPay": {
      "TmnCode": "ATSL5MUY",
      "HashSecret": "UYCBLK06MOOR5QBQLVIJD8RD0D0ILHY1",
      "BaseUrl": "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html",
      "ReturnUrl": "https://localhost:5173/payment-success?vendor=vnpay",
      // "IpnUrl": "https://localhost:7073/api/payments/ipn?vendor=vnpay", // Comment out for local development
      "Locale": "vn",
      "CurrCode": "VND"
    }
  }
}
```

**Lưu ý:** IPN URL được comment lại cho môi trường local development vì VNPay không thể gọi được localhost. Khi deploy production, hãy uncomment và cấu hình đúng URL public.

## Flow thanh toán

1. **Frontend** gọi `/api/payments/create-url-for-appointment` với thông tin cuộc hẹn
2. **Backend** tính toán số tiền dựa trên `PricePerHour` của luật sư và `durationHours`
3. **Backend** tạo URL thanh toán VNPay và trả về cho frontend
4. **Frontend** redirect user đến URL thanh toán VNPay
5. **User** thực hiện thanh toán trên VNPay
6. **VNPay** redirect về `/api/payments/return` và gửi IPN đến `/api/payments/ipn`
7. **Backend** xử lý callback, cập nhật trạng thái thanh toán và tạo transaction records
8. **Frontend** có thể kiểm tra trạng thái thanh toán qua `/api/payments/status/{orderId}`

## Transaction Service

Sau khi thanh toán thành công, hệ thống sẽ:
- Cập nhật trạng thái payment
- Tạo transaction records (có thể mở rộng để cập nhật balance, gửi email, etc.)
- Log thông tin giao dịch

## Error Handling

Tất cả endpoints đều có error handling và trả về thông báo lỗi chi tiết khi có vấn đề xảy ra.
