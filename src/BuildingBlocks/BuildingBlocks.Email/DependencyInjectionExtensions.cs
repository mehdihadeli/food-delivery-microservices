using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Email.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BuildingBlocks.Email;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddEmailService(
        this IServiceCollection services,
        IConfiguration configuration,
        EmailProvider provider = EmailProvider.MimKit,
        Action<EmailOptions>? configureOptions = null
    )
    {
        var config = configuration.BindOptions<EmailOptions>(nameof(EmailOptions));
        configureOptions?.Invoke(config);

        if (provider == EmailProvider.SendGrid)
        {
            services.TryAddSingleton<IEmailSender, SendGridEmailSender>();
        }
        else
        {
            services.TryAddSingleton<IEmailSender, MailKitEmailSender>();
        }

        if (configureOptions is { })
        {
            services.Configure(nameof(EmailOptions), configureOptions);
        }
        else
        {
            services
                .AddOptions<EmailOptions>()
                .Bind(configuration.GetSection(nameof(EmailOptions)))
                .ValidateDataAnnotations();
        }

        return services;
    }
}
