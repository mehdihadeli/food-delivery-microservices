using AutoMapper;

namespace BuildingBlocks.Core.Mapping;

public static class Mapper
{
    private static readonly Func<IMapper> _autoMapperFunc =
        ServiceActivator.GetScope().ServiceProvider.GetRequiredService<IMapper>;

    public static TDestination Map<TDestination>(object source)
    {
        var mapper = _autoMapperFunc.Invoke();

        return mapper.Map<TDestination>(source);
    }

    public static IQueryable<TDestination> ProjectTo<TDestination>(IQueryable source)
    {
        var mapper = _autoMapperFunc.Invoke();

        return mapper.ProjectTo<TDestination>(source);
    }
}
