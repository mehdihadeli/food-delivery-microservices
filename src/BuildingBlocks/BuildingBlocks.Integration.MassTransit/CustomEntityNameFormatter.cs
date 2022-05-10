using Humanizer;
using MassTransit;

namespace BuildingBlocks.Integration.MassTransit;

public class CustomEntityNameFormatter : IEntityNameFormatter
{
    public string FormatEntityName<T>()
    {
       return typeof(T).Name.Underscore();
    }
}
