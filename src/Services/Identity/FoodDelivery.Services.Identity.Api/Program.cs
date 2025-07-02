using Bogus;
using BuildingBlocks.Core.Web.Extensions;
using BuildingBlocks.OpenApi.AspnetOpenApi.Extensions;
using FoodDelivery.Services.Identity;
using FoodDelivery.Services.Identity.Shared.Extensions.HostApplicationBuilderExtensions;
using FoodDelivery.Services.Identity.Shared.Extensions.WebApplicationExtensions;
using FoodDelivery.Services.Shared.Extensions;
using Spectre.Console;

AnsiConsole.Write(new FigletText("Identity Service").Centered().Color(Color.FromInt32(new Faker().Random.Int(1, 255))));

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.AddInfrastructure();

builder.AddApplicationServices();

var app = builder.Build();

if (app.Environment.IsDependencyTest())
{
    return;
}

app.MapDefaultEndpoints();

app.UseInfrastructure();

app.MapApplicationEndpoints();

app.UseAspnetOpenApi();

await app.RunAsync();
