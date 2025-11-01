# Appointment Booking Flow Diagram

## Visual Flow

### Flow 1: Booking 1 Dịch Vụ (No Payment Required)

```
┌─────────────────────────────────────────────────────────────┐
│                    POST /api/appointments/CREATE             │
│                                                              │
│  Request: { services: ["Consultation"] }                    │
└────────────────────┬────────────────────────────────────────┘
                     │
                     ▼
        ┌──────────────────────────┐
        │  AppointmentSagaService  │
        │     StartSagaAsync()     │
        └────────────┬─────────────┘
                     │
                     ▼
        ┌──────────────────────────┐
        │  Create Appointment      │
        │  Status: PENDING         │
        └────────────┬─────────────┘
                     │
                     ▼
        ┌──────────────────────────┐
        │  Deactivate Work Slot    │
        └────────────┬─────────────┘
                     │
                     ▼
        ┌──────────────────────────┐
        │  Send Email to Lawyer    │
        └────────────┬─────────────┘
                     │
                     ▼
        ┌──────────────────────────┐
        │  Saga: COMPLETED         │
        │  ✅ Done                 │
        └──────────────────────────┘
                     │
                     ▼
        ┌──────────────────────────┐
        │  Lawyer Approves         │
        │  PUT /confirm            │
        └────────────┬─────────────┘
                     ▼
        ┌──────────────────────────┐
        │  Status: CONFIRMED       │
        │  ✅ Final State          │
        └──────────────────────────┘
```

### Flow 2: Booking 2+ Dịch Vụ (Payment Required)

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                    POST /api/appointments/CREATE                             │
│                                                                              │
│  Request: { services: ["Consultation", "Document Review"] }                 │
└────────────────────┬────────────────────────────────────────────────────────┘
                     │
                     ▼
        ┌──────────────────────────┐
        │  AppointmentSagaService  │
        │     StartSagaAsync()     │
        └────────────┬─────────────┘
                     │
                     ▼
        ┌──────────────────────────┐
        │  Create Appointment      │
        │  Status: AWAITING_PAYMENT│
        └────────────┬─────────────┘
                     │
                     ▼
        ┌──────────────────────────┐
        │  Deactivate Work Slot    │
        └────────────┬─────────────┘
                     │
                     ▼
        ┌──────────────────────────┐
        │  Saga: PAYMENT_PENDING   │
        │  ⏳ Waiting for Payment  │
        └────────────┬─────────────┘
                     │
                     ▼
        ┌──────────────────────────────────────────┐
        │  POST /api/payments/create-url-for-      │
        │         appointment                       │
        │                                           │
        │  { appointmentId: 123, lawyerId: 5 }     │
        └────────────┬──────────────────────────────┘
                     │
                     ▼
        ┌──────────────────────────┐
        │  Calculate Payment       │
        │  Amount = Price * 2 * 30%│
        └────────────┬─────────────┘
                     │
                     ▼
        ┌──────────────────────────┐
        │  Create Payment Record   │
        │  Status: pending         │
        └────────────┬─────────────┘
                     │
                     ▼
        ┌──────────────────────────┐
        │  Return VNPay URL        │
        └────────────┬─────────────┘
                     │
                     ▼
        ┌──────────────────────────┐
        │  User Redirect to VNPay  │
        │  ⏳ User Processing...   │
        └────────────┬─────────────┘
                     │
                     ├──────────────┬──────────────┐
                     ▼              ▼              ▼
        ┌───────────────┐  ┌──────────────┐  ┌──────────────┐
        │  SUCCESS      │  │  FAILED      │  │  TIMEOUT     │
        │               │  │              │  │  (Future)    │
        └───────┬───────┘  └──────┬───────┘  └──────────────┘
                │                 │
                ▼                 ▼
        ┌───────────────┐  ┌───────────────────────────┐
        │ GET /return   │  │ GET /return               │
        │ status=success│  │ status=failed             │
        └───────┬───────┘  └───────────┬───────────────┘
                │                     │
                ▼                     ▼
        ┌───────────────┐  ┌───────────────────────────┐
        │ Update Payment│  │ Compensate Saga           │
        │ status=success│  │ 1. Activate Work Slot     │
        └───────┬───────┘  │ 2. Cancel Appointment     │
                │          │ 3. IsDel = true           │
                ▼          │ 4. Status = CANCELLED     │
        ┌───────────────┐  └───────────┬───────────────┘
        │ Update        │              │
        │ Appointment   │              ▼
        │ Status:       │  ┌───────────────────────────┐
        │ CONFIRMED ✅  │  │ Saga: FAILED              │
        └───────────────┘  │ User retries              │
                │          └───────────────────────────┘
                ▼
        ┌───────────────┐
        │ Saga:         │
        │ COMPLETED ✅  │
        └───────────────┘
```

## State Transitions

### Appointment Status Flow

```
Booking 1 Service:
┌────────┐      ┌───────────┐      ┌────────────┐      ┌────────────┐
│  START │ ────▶│  PENDING  │ ────▶│  CONFIRMED │ ────▶│ COMPLETED  │
└────────┘      └───────────┘      └────────────┘      └────────────┘
                  (Chờ lawyer)      (Lawyer approve)

Booking 2+ Services:
┌────────┐      ┌─────────────────┐      ┌────────────┐      ┌────────────┐
│  START │ ────▶│ AWAITING_PAYMENT│ ────▶│  CONFIRMED │ ────▶│ COMPLETED  │
└────────┘      └────────┬────────┘      └────────────┘      └────────────┘
                         │                    ▲
                         │                    │ (Payment Success)
                         ▼                    │
                  ┌─────────────┐            │
                  │   FAILED    │            │
                  │  CANCELLED  │────────────┘
                  └─────────────┘
                      (Payment Failed)
```

### Saga State Flow

```
1 Service Flow:
STARTED → WORK_SLOT_DEACTIVATED → EMAIL_SENT → COMPLETED

2+ Services Flow (Payment Success):
STARTED → WORK_SLOT_DEACTIVATED → PAYMENT_PENDING → COMPLETED

2+ Services Flow (Payment Failed):
STARTED → WORK_SLOT_DEACTIVATED → PAYMENT_PENDING → COMPENSATING → FAILED
                                              │
                                              └─▶ Activate Work Slot
                                              └─▶ Cancel Appointment
                                              └─▶ Clean Up Resources
```

## Database Relationships

```
Appointments                    Payments
┌─────────────────┐            ┌─────────────────┐
│ Id (PK)         │◄───────────┤ AppointmentId   │ (FK, nullable)
│ UserId          │            │ OrderId (PK)    │
│ LawyerId        │            │ Vendor          │
│ Status          │            │ Amount          │
│ Services        │            │ Status          │
│ ScheduledAt     │            │ TransactionId   │
│ ...             │            │ ...             │
└─────────────────┘            └─────────────────┘
```

## Error Handling & Compensation

### When Payment Fails:

1. **Transaction Rollback**:
   ```
   Payment Status → "failed"
   Appointment Status → "Cancelled"
   Appointment.IsDel → true
   ```

2. **Work Slot Reactivation**:
   ```
   Call: ActivateWorkSlotAsync(slot, dayOfWeek, lawyerId)
   → Work slot available again for booking
   ```

3. **User Notification**:
   ```
   Log error message
   Return error to frontend
   User can retry booking
   ```

### Compensation Steps:

```csharp
1. Get appointment by ID
2. Check saga state
3. If work slot was deactivated → Activate it back
4. Mark appointment as deleted (IsDel = true)
5. Update status to Cancelled
6. Log compensation reason
```

## Sequence Diagrams

### Success Flow (2+ Services)

```
User    Frontend    AppointmentAPI    PaymentAPI    VNPay    Database
 │         │              │               │          │          │
 │  POST   │              │               │          │          │
 │────────▶│  POST        │               │          │          │
 │         │─────────────▶│  Create       │          │          │
 │         │              │  Appointment  │          │          │
 │         │              │───────────────┼──────────┼─────────▶│
 │         │              │               │          │          │
 │         │              │◀──────────────┼──────────┼──────────┤
 │         │              │  AppointmentId│          │          │
 │         │              │               │          │          │
 │         │              │  Deactivate   │          │          │
 │         │              │  Work Slot    │          │          │
 │         │              │─────────────────────────────────────▶│
 │         │              │               │          │          │
 │         │              │◀─────────────────────────────────────┤
 │         │              │               │          │          │
 │         │              │◀──────────────┤          │          │
 │         │              │  AppointmentId│          │          │
 │         │◀─────────────┤  + State      │          │          │
 │         │              │               │          │          │
 │  POST   │              │               │          │          │
 │────────▶│  POST        │               │  Create  │          │
 │         │──────────────┼──────────────▶│  Payment │          │
 │         │              │               │─────────▶│          │
 │         │              │               │          │          │
 │         │              │               │◀─────────┤          │
 │         │◀─────────────┼───────────────┤  PaymentId          │
 │         │              │               │          │          │
 │  GET    │              │               │  Redirect│          │
 │────────▶│──────────────┼───────────────┼─────────▶│          │
 │         │              │               │          │          │
 │         │              │               │          │  Process │
 │         │              │               │          │◀─────────┤
 │         │              │               │          │          │
 │         │              │               │  Callback│          │
 │         │◀─────────────┼───────────────┼─────────┤          │
 │         │              │               │          │          │
 │         │              │               │  Update  │          │
 │         │              │───────────────┼──────────┼─────────▶│
 │         │              │               │          │          │
 │         │              │  Update       │          │          │
 │         │              │  Appointment  │          │          │
 │         │              │────────────────────────────────────▶│
 │         │              │               │          │          │
 │         │◀─────────────┤               │          │          │
 │  Success│              │               │          │          │
```

### Failed Flow (2+ Services)

```
User    Frontend    AppointmentAPI    PaymentAPI    VNPay    Database
 │         │              │               │          │          │
 │  ... (same until payment callback)    │          │          │
 │         │              │               │          │          │
 │         │              │               │  FAILED  │          │
 │         │◀─────────────┼───────────────┼─────────┤          │
 │         │              │               │          │          │
 │         │              │  Update       │          │          │
 │         │              │  Payment      │          │          │
 │         │              │────────────────────────────────────▶│
 │         │              │               │          │          │
 │         │              │  Compensate   │          │          │
 │         │              │  Activate     │          │          │
 │         │              │  Work Slot    │          │          │
 │         │              │────────────────────────────────────▶│
 │         │              │               │          │          │
 │         │              │  Cancel       │          │          │
 │         │              │  Appointment  │          │          │
 │         │              │────────────────────────────────────▶│
 │         │              │               │          │          │
 │         │◀─────────────┤               │          │          │
 │  Error  │              │               │          │          │
```

## Summary

| Aspect | Booking 1 Service | Booking 2+ Services |
|--------|-------------------|---------------------|
| **Initial Status** | `Pending` | `AwaitingPayment` |
| **Requires Payment** | ❌ No | ✅ Yes |
| **Email Sent** | ✅ To Lawyer | ❌ No |
| **Saga Final State** | `Completed` | `Completed` / `Failed` |
| **User Action** | Wait for approval | Complete payment |
| **On Failure** | N/A | Auto-cancel + rollback |

