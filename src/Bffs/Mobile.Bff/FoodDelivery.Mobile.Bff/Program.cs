using FoodDelivery.ServiceDefaults.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

var app = builder.Build();

app.MapDefaultEndpoints();

app.MapGet(
    "/",
    async (HttpContext context) =>
    {
        await context.Response.WriteAsync("Mobile Bff.");
    }
);

app.UseAuthentication();
app.UseAuthorization();
app.MapReverseProxy();

app.Run();
