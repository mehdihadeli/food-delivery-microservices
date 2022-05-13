using Microsoft.Extensions.Configuration;
using Tests.Shared.Helpers;

namespace Tests.Shared;

public static class OptionsHelper
{
    public static TOptions GetOptions<TOptions>(string section, string? settingsFileName = null)
        where TOptions : class, new()
    {
        var configuration = new TOptions();

        ConfigurationHelper.BuildConfiguration(settingsFileName)
            .GetSection(section)
            .Bind(configuration);

        return configuration;
    }
}
