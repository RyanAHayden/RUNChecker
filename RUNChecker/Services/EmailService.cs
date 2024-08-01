using Microsoft.Extensions.Options;
using RUNChecker.Options;
using System.Net.Mail;
using Microsoft.Extensions.Logging;

namespace RUNChecker.Services
{
    public class EmailService(ILogger<CertificateChecker> logger, IOptions<EmailServiceOptions> emailServiceOptions)
    {
        private readonly ILogger<CertificateChecker> _logger = logger;
        private readonly EmailServiceOptions _emailServiceOptions = emailServiceOptions.Value;

        public void SendEmail(string subject, string body)
        {
            SmtpClient smtpClient = new(_emailServiceOptions.MailServer)
            {
                Port = 25
            };

            MailMessage message = new()
            {
                From = new MailAddress(_emailServiceOptions.Sender)
            };

            if (_emailServiceOptions.Recipients != null)
            {
                foreach (var recipient in _emailServiceOptions.Recipients)
                {
                    message.To.Add(recipient);
                }
                message.Subject = subject;
                message.Body = body;
                message.IsBodyHtml = true;

                smtpClient.Send(message);
                _logger.LogInformation($"Email sent: {subject}");
            }
            else
            {
                _logger.LogError($"No recipients to send email to.");
            }
        }
    }
}
