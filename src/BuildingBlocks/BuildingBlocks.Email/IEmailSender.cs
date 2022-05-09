using Ardalis.GuardClauses;

namespace BuildingBlocks.Email;

public interface IEmailSender
{
    Task SendAsync(EmailObject emailObject);
}

public class EmailObject
{
    public EmailObject(string receiverEmail, string subject, string mailBody)
    {
        ReceiverEmail = Guard.Against.NullOrEmpty(receiverEmail, nameof(receiverEmail));
        Subject = Guard.Against.NullOrEmpty(subject, nameof(subject));
        MailBody = Guard.Against.NullOrEmpty(mailBody, nameof(mailBody));
    }

    public EmailObject(
        string receiverEmail,
        string senderEmail,
        string subject,
        string mailBody)
    {
        ReceiverEmail = Guard.Against.NullOrEmpty(receiverEmail, nameof(receiverEmail));
        SenderEmail = Guard.Against.NullOrEmpty(senderEmail, nameof(senderEmail));
        Subject = Guard.Against.NullOrEmpty(subject, nameof(subject));
        MailBody = Guard.Against.NullOrEmpty(mailBody, nameof(mailBody));
    }

    public string ReceiverEmail { get; }

    public string SenderEmail { get; }

    public string Subject { get; }

    public string MailBody { get; }
}
