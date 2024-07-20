using BuildingBlocks.Core.Domain.Events.Internal;
using BuildingBlocks.Core.Types;
using FluentAssertions;

namespace BuildingBlocks.Core.UnitTests;

public class TypeMapperTests
{
    [Fact]
    public void get_type_name_should_return_correct_name()
    {
        TypeMapper.GetTypeName<OrderCreated>().Should().Be(typeof(OrderCreated).FullName!.Replace(".", "_"));
        TypeMapper.GetTypeName(typeof(OrderCreated)).Should().Be(typeof(OrderCreated).FullName!.Replace(".", "_"));
        TypeMapper
            .GetTypeNameByObject(new OrderCreated())
            .Should()
            .Be(typeof(OrderCreated).FullName!.Replace(".", "_"));
    }

    [Fact]
    public void get_type_should_return_correct_type()
    {
        TypeMapper.GetType(nameof(OrderCreated)).Should().Be<OrderCreated>();
    }
}

public record OrderCreated : DomainEvent;
