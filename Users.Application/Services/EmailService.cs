using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Users.Application.Services
{
    public interface IEmailService
    {
        Task SendOtpEmailAsync(string to, string otp, string purpose);
        Task SendNotificationEmailAsync(string to, string subject, string htmlBody);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendNotificationEmailAsync(string to, string subject, string htmlBody)
        {
            try
            {
                var email = new MimeMessage();
                email.From.Add(new MailboxAddress("Law Appointment App", _configuration["EmailSettings:From"]));
                email.To.Add(new MailboxAddress("", to));
                email.Subject = subject;

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = htmlBody
                };
                email.Body = bodyBuilder.ToMessageBody();

                using var smtp = new SmtpClient();
                await smtp.ConnectAsync(
                    _configuration["EmailSettings:SmtpServer"],
                    int.Parse(_configuration["EmailSettings:Port"]),
                    SecureSocketOptions.StartTls
                );

                await smtp.AuthenticateAsync(
                    _configuration["EmailSettings:Username"],
                    _configuration["EmailSettings:Password"]
                );

                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);

                _logger.LogInformation($"Notification email sent successfully to {to}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send notification email to {to}");
                throw;
            }
        }



        public async Task SendOtpEmailAsync(string to, string otp, string purpose)
        {
            try
            {
                var email = new MimeMessage();
                email.From.Add(new MailboxAddress("Law Appointment App", _configuration["EmailSettings:From"]));
                email.To.Add(new MailboxAddress("", to));

                string subject;
                string htmlBody;

                switch (purpose.ToLower())
                {
                    case "registration":
                        subject = "Verify Your Email - Law Appointment App";
                        htmlBody = $@"
                            <h2>Email Verification</h2>
                            <p>Thank you for registering with Law Appointment App. Please use the following OTP to verify your email:</p>
                            <h1 style='color: #4CAF50; font-size: 32px;'>{otp}</h1>
                            <p>This OTP will expire in 5 minutes.</p>
                            <p>If you didn't request this verification, please ignore this email.</p>";
                        break;

                    case "forgotpassword":
                        subject = "Reset Your Password - Law Appointment App";
                        htmlBody = $@"
                            <h2>Password Reset Request</h2>
                            <p>We received a request to reset your password for Law Appointment App. Please use the following OTP to reset your password:</p>
                            <h1 style='color: #4CAF50; font-size: 32px;'>{otp}</h1>
                            <p>This OTP will expire in 5 minutes.</p>
                            <p>If you didn't request a password reset, please ignore this email and ensure your account is secure.</p>";
                        break;

                    default:
                        throw new ArgumentException("Invalid email purpose");
                }

                email.Subject = subject;

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = htmlBody
                };

                email.Body = bodyBuilder.ToMessageBody();

                using var smtp = new SmtpClient();
                await smtp.ConnectAsync(
                    _configuration["EmailSettings:SmtpServer"],
                    int.Parse(_configuration["EmailSettings:Port"]),
                    SecureSocketOptions.StartTls
                );

                await smtp.AuthenticateAsync(
                    _configuration["EmailSettings:Username"],
                    _configuration["EmailSettings:Password"]
                );

                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);

                _logger.LogInformation($"OTP email sent successfully to {to} for {purpose}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending OTP email to {to} for {purpose}");
                throw;
            }
        }
    }
} 