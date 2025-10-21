# Payments (VNPAY)

## 1) Cấu hình
- Sửa `Appointments.Services.API/appsettings.json`:
  - `Payments:VnPay:{TmnCode, HashSecret, BaseUrl, ReturnUrl, IpnUrl}`

## 2) Endpoint
- Tạo link thanh toán: `POST /api/payments/create`
```json
{
  "vendor": "vnpay",
  "orderId": "A123",
  "amount": 100000,
  "orderInfo": "Thanh toan lich hen A123"
}
```
- Tạo link thanh toán cho appointment: `POST /api/payments/create-for-appointment`
```json
{
  "vendor": "vnpay",
  "orderId": "A123",
  "lawyerId": 1,
  "durationHours": 1,
  "orderInfo": "Thanh toan lich hen A123"
}
```
- Nhận kết quả redirect: `GET /api/payments/return?vendor=vnpay&...`
- Nhận IPN (server-to-server): `GET /api/payments/ipn?vendor=vnpay&...`
- Tra cứu: `GET /api/payments/status/{orderId}`
- QR dev nhanh: `GET /api/payments/qr?url={PaymentUrl}`

## 3) Xác thực chữ ký
- VNPAY: HMAC SHA512 trên chuỗi query (bỏ `vnp_SecureHash`). Đã triển khai trong `VnPayService`.

## 4) Tích hợp Gateway
- Đã thêm route `/api/payments/{everything}` trong `API.Gateway/ocelot.json` → Appointments service (7001).

## 5) Test nhanh
1. Chạy Appointments API (https 7001) và API Gateway (https 5000).
2. Gọi `POST https://localhost:5000/api/payments/create` body như trên → nhận `paymentUrl`.
3. Mở `paymentUrl` trên trình duyệt (sandbox) → thanh toán.
4. Kiểm tra `GET /api/payments/return` và `GET /api/payments/ipn` được provider gọi lại (nhìn log).

## 6) Ghi chú triển khai thực tế
- Lưu trữ trạng thái đơn hàng (order/payment) vào DB và cập nhật tại `IPN`.
- Bảo vệ IPN: kiểm tra IP whitelist (nếu có) + verify signature.
- Dùng thư viện QR nội bộ thay vì Google Chart nếu cần kiểm soát.
- Cấu hình domain thật (https) cho `ReturnUrl` và `IpnUrl` trong production.

