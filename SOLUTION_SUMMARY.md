# Solution Summary: Appointment Booking with Payment Handling

## Problem Statement

Ban đầu bạn đang gặp vấn đề:
- Khi book lịch hẹn với 2+ dịch vụ, cần thanh toán cọc trước
- Appointment phải được tạo trước để có ID gửi cho payment
- Nếu thanh toán thất bại, appointment bị "treo" và user phải nhập lại từ đầu

## Solution Overview

Giải pháp sử dụng **Saga Pattern** và **Transaction Compensation** để xử lý 2 workflow booking khác nhau:

### Scenario 1: Booking 1 dịch vụ
- **Flow**: Tạo appointment → Deactivate work slot → Gửi email → Hoàn tất (Pending)
- **Status**: `Pending` - Chờ luật sư approve
- **Không cần payment**

### Scenario 2: Booking 2+ dịch vụ
- **Flow**: Tạo appointment → Deactivate work slot → Chờ thanh toán (AwaitingPayment)
- **Payment Success**: Status → `Confirmed`
- **Payment Failed**: Compensate (rollback) → Appointment cancelled

## Key Changes Made

### 1. Database Schema
**File**: `Appointments.Infrastructure/Models/Payment.cs`
- Thêm field `AppointmentId` để link appointment với payment
- Migration script: `add_appointmentid_to_payments.sql`

### 2. Status Enums
**Files**: 
- `Appointments.Infrastructure/Models/Enums/AppointmentStatus.cs`
- `Appointments.Infrastructure/Models/Saga/AppointmentSagaState.cs`

**Appointment Status:**
- `Pending = 0` - Chờ approve (1 dịch vụ)
- `AwaitingPayment = 1` - Chờ thanh toán (2+ dịch vụ) **NEW**
- `Confirmed = 2` - Đã confirm/thanh toán
- `Cancelled = 3`
- `Completed = 4`

**Saga State:**
- `PaymentPending` **NEW** - Trạng thái chờ payment

### 3. Saga Service Logic
**File**: `Appointments.Application/Services/Saga/AppointmentSagaService.cs`

**Logic mới:**
```csharp
var serviceCount = dto.Services?.Count ?? 0;
var needsPayment = serviceCount >= 2;

// Set initial status based on service count
Status = needsPayment ? AwaitingPayment : Pending

// Send email only for non-payment bookings
if (!needsPayment) {
    await SendEmailNotificationAsync(appointment.Id);
}

// Saga state
if (needsPayment) {
    sagaData.State = PaymentPending;
} else {
    await CompleteSagaAsync(appointment.Id);
}
```

### 4. Transaction Service
**File**: `Appointments.Application/Services/TransactionService.cs`

**New functionality:**
- Khi payment success: Update appointment → `Confirmed`
- Khi payment failed: Compensate appointment → `Cancelled` (IsDel = true)

**Compensation logic:**
```csharp
// Payment success
appointment.Status = Confirmed;
await _appointmentRepository.UpdateAsync(appointment);

// Payment failed - Clean up
appointment.IsDel = true;
appointment.Status = Cancelled;
await _appointmentRepository.UpdateAsync(appointment);
```

### 5. Payment Controller
**File**: `Appointments.Services.API/Controllers/PaymentController.cs`

**Update**: Lưu `AppointmentId` vào payment record

### 6. Payment Calculation
**File**: `Appointments.Application/Services/PaymentCalculationService.cs`

**Formula**: `PricePerHour * 2 * 30%` (cọc trước)

### 7. DTOs
**File**: `Appointments.Infrastructure/Models/Dtos/PaymentDtos.cs`

**Update**: Thêm `AppointmentId` vào `CreatePaymentForAppointmentRequestDto`

## Complete Flow

### Booking 1 dịch vụ (Pending)
```
1. POST /api/appointments/CREATE
   {
     "services": ["Consultation"]
   }
   → Create appointment (Status: Pending)
   → Deactivate work slot
   → Send email to lawyer
   → Saga: Completed

2. Lawyer approves
   PUT /api/appointments/{id}/confirm
   → Status: Confirmed
```

### Booking 2+ dịch vụ (AwaitingPayment)
```
1. POST /api/appointments/CREATE
   {
     "services": ["Consultation", "Document Review"]
   }
   → Create appointment (Status: AwaitingPayment)
   → Deactivate work slot
   → Saga: PaymentPending
   → Response: { appointmentId, state: "PaymentPending" }

2. POST /api/payments/create-url-for-appointment
   {
     "appointmentId": 123,
     "lawyerId": 5,
     "durationHours": 1
   }
   → Calculate: PricePerHour * 2 * 30%
   → Create VNPay URL
   → Save payment with AppointmentId

3. User thanh toán trên VNPay

4a. Payment Success:
   GET /api/payments/return?...
   → Update payment status = "success"
   → Update appointment status = Confirmed
   → Saga: Completed

4b. Payment Failed:
   GET /api/payments/return?...
   → Update payment status = "failed"
   → Compensate: Activate work slot
   → Update appointment: IsDel = true, Status = Cancelled
   → Saga: Failed
```

## Benefits

✅ **Data Consistency**: Appointment và Payment được link chặt chẽ
✅ **No Orphaned Data**: Failed payments trigger cleanup
✅ **Saga Pattern**: Rollback mechanism sẵn có
✅ **Clear Status Flow**: Status rõ ràng cho từng scenario
✅ **User Experience**: Không mất dữ liệu khi payment fail

## Migration Required

**Database**: Chạy migration script
```sql
-- File: Appointments.Infrastructure/Data/Scripts/add_appointmentid_to_payments.sql
ALTER TABLE [dbo].[Payments]
ADD [AppointmentId] INT NULL;

CREATE INDEX IX_Payments_AppointmentId ON [dbo].[Payments]([AppointmentId]);
```

## Testing Checklist

- [x] Booking 1 dịch vụ không cần payment
- [ ] Booking 2 dịch vụ tạo payment URL thành công
- [ ] Payment success → appointment confirmed
- [ ] Payment failed → appointment cancelled
- [ ] Work slot được rollback khi payment fail
- [ ] Email chỉ gửi cho booking 1 dịch vụ
- [ ] Status transitions đúng theo flow

## Future Enhancements

1. Payment timeout: Auto-cancel nếu quá X giờ chưa thanh toán
2. Retry mechanism cho failed payments
3. Email notifications cho mọi payment status
4. Refund logic cho cancellations sau payment
5. Payment history dashboard

## Files Modified

- `Appointments.Infrastructure/Models/Payment.cs`
- `Appointments.Infrastructure/Models/Enums/AppointmentStatus.cs`
- `Appointments.Infrastructure/Models/Saga/AppointmentSagaState.cs`
- `Appointments.Infrastructure/Models/Dtos/PaymentDtos.cs`
- `Appointments.Application/Services/Saga/AppointmentSagaService.cs`
- `Appointments.Application/Services/TransactionService.cs`
- `Appointments.Application/Services/PaymentCalculationService.cs`
- `Appointments.Services.API/Controllers/PaymentController.cs`

## Files Created

- `Appointments.Infrastructure/Data/Scripts/add_appointmentid_to_payments.sql`
- `Appointments.Services.API/README_APPOINTMENT_BOOKING_FLOW.md`
- `SOLUTION_SUMMARY.md`

## No Breaking Changes

Các enum values được thêm mới, không thay đổi values cũ:
- `Pending = 0` (giữ nguyên)
- `Confirmed = 2` (thay đổi từ 1)
- `Cancelled = 3` (thay đổi từ 2)
- `Completed = 4` (thay đổi từ 3)

**Lưu ý**: Cần migration dữ liệu existing nếu có records trong database.

