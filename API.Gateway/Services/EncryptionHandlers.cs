using Microsoft.AspNetCore.DataProtection;
using System.Text;
using System.Text.Json;

namespace API.Gateway.Services
{
    public class RequestEncryptionHandler : DelegatingHandler
    {
        private readonly IDataProtector _protector;

        public RequestEncryptionHandler(IDataProtectionProvider protectionProvider)
        {
            _protector = protectionProvider.CreateProtector("RequestEncryption");
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Encrypt sensitive request data if needed
            if (request.Content != null && ShouldEncrypt(request))
            {
                var content = await request.Content.ReadAsStringAsync();
                var encryptedContent = _protector.Protect(content);
                
                request.Content = new StringContent(encryptedContent, Encoding.UTF8, "application/json");
                request.Headers.Add("X-Encrypted", "true");
            }

            return await base.SendAsync(request, cancellationToken);
        }

        private bool ShouldEncrypt(HttpRequestMessage request)
        {
            // Only encrypt sensitive endpoints
            var sensitiveEndpoints = new[] { "/api/users/register", "/api/users/login", "/api/payments" };
            return sensitiveEndpoints.Any(endpoint => request.RequestUri?.AbsolutePath.Contains(endpoint) == true);
        }
    }

    public class ResponseDecryptionHandler : DelegatingHandler
    {
        private readonly IDataProtector _protector;

        public ResponseDecryptionHandler(IDataProtectionProvider protectionProvider)
        {
            _protector = protectionProvider.CreateProtector("ResponseDecryption");
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = await base.SendAsync(request, cancellationToken);

            // Decrypt response if it was encrypted
            if (response.Headers.Contains("X-Encrypted"))
            {
                var content = await response.Content.ReadAsStringAsync();
                try
                {
                    var decryptedContent = _protector.Unprotect(content);
                    response.Content = new StringContent(decryptedContent, Encoding.UTF8, "application/json");
                }
                catch
                {
                    // If decryption fails, return original content
                }
            }

            return response;
        }
    }
}
