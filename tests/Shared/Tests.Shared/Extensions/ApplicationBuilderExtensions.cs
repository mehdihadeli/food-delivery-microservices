using Microsoft.AspNetCore.Builder;

namespace Tests.Shared.Extensions;

//https://github.com/dotnet/aspnetcore/issues/37680#issuecomment-1331559463
//https://github.com/dotnet/aspnetcore/issues/45319#issuecomment-1334355103
public static class ApplicationBuilderExtensions
{
    // This async local is set in from tests and it flows to main
    private static readonly AsyncLocal<Action<IApplicationBuilder>?> _current = new();

    /// <summary>
    /// Adds the current test application builder to the application in the "right" place
    /// </summary>
    /// <param name="app">The application builder</param>
    /// <returns>The modified <see cref="IApplicationBuilder"/></returns>
    public static IApplicationBuilder AddTestApplicationBuilder(this IApplicationBuilder app)
    {
        if (_current.Value is { } configure)
        {
            configure(app);
        }

        return app;
    }

    /// <summary>
    /// Unit tests can use this to flow state to the main program and change application builder
    /// </summary>
    /// <param name="action"></param>
    public static void ConfigureTestApplicationBuilder(this Action<IApplicationBuilder> action)
    {
        _current.Value = action;
    }
}
