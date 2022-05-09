using Ardalis.GuardClauses;
using BuildingBlocks.Email.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace BuildingBlocks.Email;

// https://swimburger.net/blog/dotnet/send-emails-using-the-sendgrid-api-with-dotnet-6-and-csharp
public class SendGridEmailSender : IEmailSender
{
    private readonly ILogger<SendGridEmailSender> _logger;
    private readonly EmailOptions _config;

    public SendGridEmailSender(IOptions<EmailOptions> emailConfig, ILogger<SendGridEmailSender> logger)
    {
        _logger = logger;
        _config = Guard.Against.Null(emailConfig?.Value, nameof(EmailOptions));
    }

    private SendGridClient SendGridClient => new(_config.SendGridOptions.ApiKey);

    public async Task SendAsync(EmailObject emailObject)
    {
        Guard.Against.Null(emailObject, nameof(EmailObject));

        var message = new SendGridMessage
        {
            Subject = emailObject.Subject,
            HtmlContent = emailObject.MailBody,
        };

        message.AddTo(new EmailAddress(emailObject.ReceiverEmail));

        message.From = new EmailAddress(emailObject.SenderEmail ?? _config.From);
        message.ReplyTo = new EmailAddress(emailObject.SenderEmail ?? _config.From);

        await SendGridClient.SendEmailAsync(message);

        _logger.LogInformation(
            "Email sent. From: {From}, To: {To}, Subject: {Subject}, Content: {Content}",
            _config.From,
            emailObject.ReceiverEmail,
            emailObject.Subject,
            emailObject.MailBody);
    }
}
