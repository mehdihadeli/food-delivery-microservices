using AutoBogus;
using AutoMapper;
using FluentAssertions;
using FoodDelivery.Services.Customers.RestockSubscriptions;
using FoodDelivery.Services.Customers.RestockSubscriptions.Dtos.v1;
using FoodDelivery.Services.Customers.RestockSubscriptions.Features.CreatingRestockSubscription.v1;
using FoodDelivery.Services.Customers.RestockSubscriptions.Features.ProcessingRestockNotification.v1;
using FoodDelivery.Services.Customers.RestockSubscriptions.Models.Read;
using FoodDelivery.Services.Customers.TestShared.Fakes.RestockSubscriptions.Entities;
using FoodDelivery.Services.Customers.UnitTests.Common;
using Tests.Shared.XunitCategories;

namespace FoodDelivery.Services.Customers.UnitTests.RestockSubscriptions;

public class RestockSubscriptionMappingTests
{
    [Fact]
    [CategoryTrait(TestCategory.Unit)]
    public void can_map_restock_subscription_to_restock_subscription_dto()
    {
        var restockSubscription = new FakeRestockSubscriptions().Generate();
        var dto = restockSubscription.ToRestockSubscriptionDto();
        restockSubscription.CustomerId.Value.Should().Be(dto.CustomerId);
        restockSubscription.Email.Value.Should().Be(dto.Email);
    }

    [Fact]
    [CategoryTrait(TestCategory.Unit)]
    public void can_map_restock_subscription_to_restock_subscription_read()
    {
        var restockSubscription = new FakeRestockSubscriptions().Generate();
        var readModel = restockSubscription.ToRestockSubscription();
        restockSubscription.CustomerId.Value.Should().Be(readModel.CustomerId);
        restockSubscription.Email.Value.Should().Be(readModel.Email);
    }

    [Fact]
    [CategoryTrait(TestCategory.Unit)]
    public void can_map_restock_subscription_read_to_restock_subscription_dto()
    {
        var restockSubscriptionRead = AutoFaker.Generate<RestockSubscription>();
        var dto = restockSubscriptionRead.ToRestockSubscriptionDto();
        restockSubscriptionRead.RestockSubscriptionId.Should().Be(dto.Id);
        restockSubscriptionRead.CustomerId.Should().Be(dto.CustomerId);
    }

    [Fact]
    [CategoryTrait(TestCategory.Unit)]
    public void can_map_create_restock_subscription_read_to_restock_subscription_read()
    {
        var createMongoRestockSubscriptionRead = AutoFaker.Generate<CreateMongoRestockSubscriptionReadModels>();
        var readModel = createMongoRestockSubscriptionRead.ToRestockSubscription();

        createMongoRestockSubscriptionRead.RestockSubscriptionId.Should().Be(readModel.RestockSubscriptionId);
        createMongoRestockSubscriptionRead.CustomerId.Should().Be(readModel.CustomerId);
    }

    // [Fact]
    // [CategoryTrait(TestCategory.Unit)]
    // public void can_map_update_restock_subscription_read_to_restock_subscription_read()
    // {
    //     var updateMongoRestockSubscriptionRead = new UpdateMongoRestockSubscriptionReadModel(
    //         new FakeRestockSubscriptions().Generate(),
    //         false
    //     );
    //     var readModel = _mapper.Map<RestockSubscription>(updateMongoRestockSubscriptionRead);
    //
    //     updateMongoRestockSubscriptionRead.RestockSubscription.Id.Value.Should().Be(readModel.RestockSubscriptionId);
    //     updateMongoRestockSubscriptionRead.RestockSubscription.CustomerId.Value.Should().Be(readModel.CustomerId);
    // }
}
