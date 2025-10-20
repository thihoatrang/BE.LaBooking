using Appointments.Infrastructure.Models.Dtos;

namespace Appointments.Application.Services.IService
{
    public interface IPaymentProvider
    {
        string Vendor { get; }
        Task<CreatePaymentResponseDto> CreateAsync(CreatePaymentRequestDto request, CancellationToken ct = default);
        bool VerifySignature(IDictionary<string, string> parameters, string signatureParamName);
        PaymentStatusDto ParseCallback(IDictionary<string, string> parameters);
    }
}


