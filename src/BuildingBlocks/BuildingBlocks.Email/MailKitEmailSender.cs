using BuildingBlocks.Email.Options;
using MailKit.Net.Smtp;
using MailKit.Security;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace BuildingBlocks.Email;

public class MailKitEmailSender(IOptions<EmailOptions> config, ILogger<MailKitEmailSender> logger) : IEmailSender
{
    private readonly EmailOptions _config = config.Value;

    public async Task SendAsync(EmailObject emailObject)
    {
        try
        {
            if (_config.MimeKitOptions is null)
            {
                throw new Exception("MimeKitOptions is empty.");
            }

            var email = new MimeMessage { Sender = MailboxAddress.Parse(emailObject.SenderEmail ?? _config.From) };
            email.To.Add(MailboxAddress.Parse(emailObject.ReceiverEmail));
            email.Subject = emailObject.Subject;
            var builder = new BodyBuilder { HtmlBody = emailObject.MailBody };
            email.Body = builder.ToMessageBody();
            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(
                _config.MimeKitOptions.Host,
                _config.MimeKitOptions.Port,
                SecureSocketOptions.StartTls
            );
            await smtp.AuthenticateAsync(_config.MimeKitOptions.UserName, _config.MimeKitOptions.Password);
            smtp.DeliveryStatusNotificationType = DeliveryStatusNotificationType.Full;
            var response = await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);

            logger.LogInformation(
                "Email sent. From: {From}, To: {To}, Subject: {Subject}, Content: {Content}",
                _config.From,
                emailObject.ReceiverEmail,
                emailObject.Subject,
                emailObject.MailBody
            );
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message, ex);
        }
    }
}
