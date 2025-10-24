using System;
using System.Collections.Generic;

namespace Appointments.Infrastructure.Models;

public partial class Payment
{
    public int Id { get; set; }

    public string OrderId { get; set; } = null!;

    public string Vendor { get; set; } = null!;

    public long? Amount { get; set; }

    public string Status { get; set; } = null!;

    public string? TransactionId { get; set; }

    public string? Message { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? BankCode { get; set; }

    public string? BankTranNo { get; set; }

    public string? PayDate { get; set; }

    public string? OrderInfo { get; set; }
}
