using AutoBogus;
using FluentAssertions;
using FoodDelivery.Services.Customers.Customers;
using FoodDelivery.Services.Customers.Customers.Features.CreatingCustomer.v1.Events.Internal.Mongo;
using FoodDelivery.Services.Customers.Customers.Features.UpdatingCustomer.v1.Events.Internal.Mongo;
using FoodDelivery.Services.Customers.Customers.Models.Reads;
using FoodDelivery.Services.Customers.TestShared.Fakes.Customers.Models;
using Tests.Shared.XunitCategories;

namespace FoodDelivery.Services.Customers.UnitTests.Customers;

public class CustomersMappingTests
{
    [Fact]
    [CategoryTrait(TestCategory.Unit)]
    public void can_map_customer_read_model_to_customer_read_dto()
    {
        var customerReadModel = AutoFaker.Generate<CustomerReadModel>();
        var res = customerReadModel.ToCustomerReadDto();
        customerReadModel.CustomerId.Should().Be(res.CustomerId);
        customerReadModel.FullName.Should().Be(res.Name);
    }

    [Fact]
    [CategoryTrait(TestCategory.Unit)]
    public void can_map_customer_to_create_mongo_customer_read_models()
    {
        var customer = new FakeCustomer().Generate();
        var res = customer.ToCreateCustomerRead();
        customer.Id.Value.Should().Be(res.CustomerId);
        customer.Name.FullName.Should().Be(res.FullName);
    }

    [Fact]
    [CategoryTrait(TestCategory.Unit)]
    public void can_map_create_mongo_customer_read_models_to_customer_read_model()
    {
        var createReadCustomer = AutoFaker.Generate<CreateCustomerRead>();
        var res = createReadCustomer.ToCustomerReadModel();
        createReadCustomer.IdentityId.Should().Be(res.IdentityId);
        createReadCustomer.CustomerId.Should().Be(res.CustomerId);
    }

    [Fact]
    [CategoryTrait(TestCategory.Unit)]
    public void can_map_customer_to_update_mongo_customer_read_model()
    {
        var customer = new FakeCustomer().Generate();
        var res = customer.ToCreateCustomerRead();
        customer.Id.Value.Should().Be(res.CustomerId);
        customer.Name.FullName.Should().Be(res.FullName);
    }

    [Fact]
    [CategoryTrait(TestCategory.Unit)]
    public void can_map_update_mongo_customer_reads_model_to_customer_read_model()
    {
        var updateMongoCustomerReadsModel = AutoFaker.Generate<UpdateCustomerRead>();
        var res = updateMongoCustomerReadsModel.ToCustomerReadModel();
        updateMongoCustomerReadsModel.Id.Should().Be(res.Id);
        updateMongoCustomerReadsModel.CustomerId.Should().Be(res.CustomerId);
    }
}
