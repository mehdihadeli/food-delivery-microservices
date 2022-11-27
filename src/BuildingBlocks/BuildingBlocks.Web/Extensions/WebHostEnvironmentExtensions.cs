using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace BuildingBlocks.Web.Extensions;

public static class WebHostEnvironmentExtensions
{
    public static bool IsTest(this IWebHostEnvironment env) => env.IsEnvironment("test");
}
