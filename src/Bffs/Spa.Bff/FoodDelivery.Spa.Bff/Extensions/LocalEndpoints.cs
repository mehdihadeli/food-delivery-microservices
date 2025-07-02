namespace FoodDelivery.Spa.Bff.Extensions;

public static class LocalEndpoints
{
    public static WebApplication MapAllLocalEndpoints(this WebApplication app)
    {
        // map all aggregation local endpoints for bff
        // use dedicated microservices client for aggregation, not local yarp
        return app;
    }
}
