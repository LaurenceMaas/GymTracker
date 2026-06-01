using System.Net;
using System.Net.Mail;
using GymTracker.Data;
using GymTracker.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GymTracker.Services
{
    public class SmtpEmailSender : IEmailSender<ApplicationUser>
    {
        private readonly EmailSettings _settings;
        private readonly ILogger<SmtpEmailSender> _logger;

        public SmtpEmailSender(
            IOptions<EmailSettings> options,
            ILogger<SmtpEmailSender> logger)
        {
            _settings = options.Value;
            _logger = logger;
        }

        public async Task SendConfirmationLinkAsync(
            ApplicationUser user,
            string email,
            string confirmationLink)
        {
            await SendEmailAsync(
                email,
                "Confirm your email",
                $"Click here: {confirmationLink}");
        }

        public async Task SendPasswordResetLinkAsync(
            ApplicationUser user,
            string email,
            string resetLink)
        {
            await SendEmailAsync(
                email,
                "Reset password",
                $"Reset here: {resetLink}");
        }

        public async Task SendPasswordResetCodeAsync(
            ApplicationUser user,
            string email,
            string resetCode)
        {
            await SendEmailAsync(
                email,
                "Reset code",
                $"Code: {resetCode}");
        }

        private async Task SendEmailAsync(
            string to,
            string subject,
            string body)
        {
            try
            {
                using var client = new SmtpClient(_settings.Host, _settings.Port)
                {
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(
                        _settings.UserName,
                        _settings.Password),
                    EnableSsl = _settings.EnableSsl,
                    Timeout = 15_000
                };

                using var mail = new MailMessage(
                    _settings.From,
                    to,
                    subject,
                    body);

                await client.SendMailAsync(mail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Recipient}", to);
                throw;
            }
        }
    }
}