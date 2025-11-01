# Appointment Booking Flow Documentation

## Overview

Hệ thống hỗ trợ 2 loại booking lịch hẹn với luật sư:

1. **Booking 1 dịch vụ**: Không cần thanh toán, chờ luật sư approve (Pending)
2. **Booking 2+ dịch vụ**: Cần thanh toán cọc 30%, status Approved khi thanh toán thành công

## Business Rules

### Booking 1 dịch vụ
- Không cần thanh toán
- Status: `Pending`
- Luật sư phải approve
- Email notification được gửi cho luật sư

### Booking 2+ dịch vụ
- Yêu cầu thanh toán cọc trước
- Status: `AwaitingPayment` (chờ thanh toán)
- Công thức tính tiền: `PricePerHour * 2 * 30%`
- Sau khi thanh toán thành công: Status → `Confirmed`
- Nếu thanh toán thất bại: Appointment bị hủy (IsDel = true, Status = Cancelled)

## Database Schema Changes

### Payment Model
Thêm field mới để liên kết với Appointment:
```csharp
public int? AppointmentId { get; set; } // Link to appointment
```

### Appointment Status Enum
```csharp
public enum AppointmentStatus
{
    Pending = 0,              // Chờ luật sư approve (1 dịch vụ)
    AwaitingPayment = 1,      // Chờ thanh toán (2+ dịch vụ)
    Confirmed = 2,            // Đã approve hoặc thanh toán thành công
    Cancelled = 3,
    Completed = 4
}
```

### Saga State Enum
```csharp
public enum AppointmentSagaState
{
    Started,
    WorkSlotDeactivated,
    EmailSent,
    PaymentPending, // Waiting for payment
    Completed,
    Failed,
    Compensating
}
```

## API Flow

### 1. Create Appointment

**POST** `/api/appointments/CREATE`

Request body:
```json
{
  "userId": 1,
  "lawyerId": 5,
  "scheduledAt": "2024-01-15T10:00:00",
  "slot": "10:00-11:00",
  "services": ["Service 1", "Service 2"],  // 2 services = need payment
  "spec": "Legal consultation",
  "note": "Important case"
}
```

Response:
```json
{
  "appointmentId": 123,
  "state": "PaymentPending",
  "message": "Appointment created successfully using Saga Pattern"
}
```

### 2. Create Payment URL (for 2+ services)

**POST** `/api/payments/create-url-for-appointment`

Request body:
```json
{
  "vendor": "vnpay",
  "orderId": "order-123",
  "lawyerId": 5,
  "durationHours": 1,
  "appointmentId": 123,
  "orderInfo": "Thanh toan coc",
  "returnUrl": "https://yoursite.com/payment-success"
}
```

Response:
```
"https://sandbox.vnpayment.vn/paymentv2/vpcpay.html?..."
```

### 3. Payment Callback

**GET** `/api/payments/return?vendor=vnpay&...`

Hệ thống tự động:
- Nếu thanh toán thành công: Update appointment status → `Confirmed`
- Nếu thanh toán thất bại: Cancel appointment (IsDel = true, Status = `Cancelled`)

## Saga Pattern Flow

### Flow cho 1 dịch vụ:
```
Started → WorkSlotDeactivated → EmailSent → Completed
```

### Flow cho 2+ dịch vụ:
```
Started → WorkSlotDeactivated → PaymentPending
                                ↓
                        [Payment Success] → Confirmed
                        [Payment Failed] → Cancelled (compensate)
```

## Compensation

Khi thanh toán thất bại hoặc có lỗi:
1. Activate lại work slot (rollback)
2. Đánh dấu appointment bị xóa (IsDel = true)
3. Update status → Cancelled

## Important Notes

1. **Payment Calculation**: Luôn tính 30% của `PricePerHour * 2 hours` (không phụ thuộc vào durationHours thực tế)
2. **Work Slot**: Luôn được deactivate khi create appointment (dù có cần payment hay không)
3. **Email Notification**: Chỉ gửi cho luật sư khi booking 1 dịch vụ (Pending)
4. **Data Consistency**: Appointment và Payment được link qua `AppointmentId`

## Migration

Chạy script SQL để thêm column:
```sql
-- File: add_appointmentid_to_payments.sql
ALTER TABLE [dbo].[Payments]
ADD [AppointmentId] INT NULL;

CREATE INDEX IX_Payments_AppointmentId ON [dbo].[Payments]([AppointmentId]);
```

## Testing

### Test Case 1: 1 dịch vụ (không cần payment)
```bash
POST /api/appointments/CREATE
{
  "services": ["Consultation"]
}
# Expected: Status = Pending, Saga = Completed
```

### Test Case 2: 2 dịch vụ (cần payment)
```bash
1. POST /api/appointments/CREATE
   {
     "services": ["Consultation", "Document Review"]
   }
   # Expected: Status = AwaitingPayment, Saga = PaymentPending

2. POST /api/payments/create-url-for-appointment
   {
     "appointmentId": 123,
     ...
   }
   # Get payment URL

3. Simulate payment success
   # Expected: Status = Confirmed

4. Simulate payment failed
   # Expected: Status = Cancelled, IsDel = true
```

## Future Enhancements

- [ ] Add email notification when payment succeeds
- [ ] Add retry mechanism for failed payments
- [ ] Add payment timeout (auto-cancel if not paid within X hours)
- [ ] Add refund logic for cancellations after payment
- [ ] Support partial payment/completion workflow

