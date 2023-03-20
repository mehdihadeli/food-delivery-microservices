using AutoBogus;
using AutoMapper;
using ECommerce.Services.Customers.Customers.Dtos.v1;
using ECommerce.Services.Customers.Customers.Features.CreatingCustomer.v1;
using ECommerce.Services.Customers.Customers.Features.UpdatingCustomer.v1;
using ECommerce.Services.Customers.Customers.Models.Reads;
using ECommerce.Services.Customers.TestShared.Fakes.Customers.Entities;
using ECommerce.Services.Customers.UnitTests.Common;
using FluentAssertions;
using Tests.Shared.XunitCategories;

namespace ECommerce.Services.Customers.UnitTests.Customers;

public class CustomersMappingTests : IClassFixture<MappingFixture>
{
    private readonly IMapper _mapper;

    public CustomersMappingTests(MappingFixture fixture)
    {
        _mapper = fixture.Mapper;
    }

    [Fact]
    [CategoryTrait(TestCategory.Unit)]
    public void must_success_with_valid_configuration()
    {
        _mapper.ConfigurationProvider.AssertConfigurationIsValid();
    }

    [Fact]
    [CategoryTrait(TestCategory.Unit)]
    public void can_map_customerreadmodel_to_customerreaddto()
    {
        var customerReadModel = AutoFaker.Generate<CustomerReadModel>();
        var res = _mapper.Map<CustomerReadDto>(customerReadModel);
        customerReadModel.CustomerId.Should().Be(res.CustomerId);
        customerReadModel.FullName.Should().Be(res.Name);
    }

    [Fact]
    [CategoryTrait(TestCategory.Unit)]
    public void can_map_customer_to_createmongocustomerreadmodels()
    {
        var customer = new FakeCustomer().Generate();
        var res = _mapper.Map<CreateMongoCustomerReadModels>(customer);
        customer.Id.Value.Should().Be(res.CustomerId);
        customer.Name.FullName.Should().Be(res.FullName);
    }

    [Fact]
    [CategoryTrait(TestCategory.Unit)]
    public void can_map_createmongocustomerreadmodels_to_customerreadmodel()
    {
        var createReadCustomer = AutoFaker.Generate<CreateMongoCustomerReadModels>();
        var res = _mapper.Map<CustomerReadModel>(createReadCustomer);
        createReadCustomer.Id.Should().Be(res.Id);
        createReadCustomer.CustomerId.Should().Be(res.CustomerId);
    }

    [Fact]
    [CategoryTrait(TestCategory.Unit)]
    public void can_map_customer_to_updatemongocustomerreadmodel()
    {
        var customer = new FakeCustomer().Generate();
        var res = _mapper.Map<UpdateMongoCustomerReadsModel>(customer);
        customer.Id.Value.Should().Be(res.CustomerId);
        customer.Name.FullName.Should().Be(res.FullName);
    }

    [Fact]
    [CategoryTrait(TestCategory.Unit)]
    public void can_map_updatemongocustomerreadsmodel_to_customerreadmodel()
    {
        var updateMongoCustomerReadsModel = AutoFaker.Generate<UpdateMongoCustomerReadsModel>();
        var res = _mapper.Map<CustomerReadModel>(updateMongoCustomerReadsModel);
        updateMongoCustomerReadsModel.Id.Should().Be(res.Id);
        updateMongoCustomerReadsModel.CustomerId.Should().Be(res.CustomerId);
    }
}
