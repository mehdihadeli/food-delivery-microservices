namespace BuildingBlocks.Email.Options;

public class EmailOptions
{
    public MailKitOptions? MimeKitOptions { get; set; }
    public SendGridOptions? SendGridOptions { get; set; }
    public string? From { get; set; }
    public string? DisplayName { get; set; }
    public bool Enable { get; set; }
}

public class MailKitOptions
{
    public string? Host { get; set; }
    public int Port { get; set; }
    public string? UserName { get; set; }
    public string? Password { get; set; }
}

public class SendGridOptions
{
    public string? ApiKey { get; set; }
}
