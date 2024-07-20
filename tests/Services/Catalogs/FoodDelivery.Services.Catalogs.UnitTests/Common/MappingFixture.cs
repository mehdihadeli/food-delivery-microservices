using AutoMapper;

namespace FoodDelivery.Services.Catalogs.UnitTests.Common;

public class MappingFixture
{
    public MappingFixture()
    {
        Mapper = MapperFactory.Create();
    }

    public IMapper Mapper { get; }
}
