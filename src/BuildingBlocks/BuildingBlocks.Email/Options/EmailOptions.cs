namespace BuildingBlocks.Email.Options;

public class EmailOptions
{
    public MailKitOptions? MimeKitOptions { get; set; }
    public SendGridOptions? SendGridOptions { get; set; }
    public string? From { get; set; }
    public string? DisplayName { get; set; }
    public bool Enable { get; set; }
}
