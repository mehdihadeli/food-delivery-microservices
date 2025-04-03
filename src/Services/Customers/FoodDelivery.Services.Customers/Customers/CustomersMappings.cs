using FoodDelivery.Services.Customers.Customers.Dtos.v1;
using FoodDelivery.Services.Customers.Customers.Features.CreatingCustomer.v1.Events.Domain;
using FoodDelivery.Services.Customers.Customers.Features.CreatingCustomer.v1.Events.Internal.Mongo;
using FoodDelivery.Services.Customers.Customers.Features.UpdatingCustomer.v1;
using FoodDelivery.Services.Customers.Customers.Features.UpdatingCustomer.v1.Events.Domain;
using FoodDelivery.Services.Customers.Customers.Features.UpdatingCustomer.v1.Events.Internal.Mongo;
using FoodDelivery.Services.Customers.Customers.Models.Reads;
using Riok.Mapperly.Abstractions;

namespace FoodDelivery.Services.Customers.Customers;

// https://mapperly.riok.app/docs/configuration/static-mappers/
[Mapper]
internal static partial class CustomersMappings
{
    [MapProperty(nameof(CustomerCreated.Id), nameof(CreateCustomerRead.CustomerId))]
    [MapProperty(nameof(CustomerCreated.Address), nameof(CreateCustomerRead.DetailAddress))]
    [MapperIgnoreSource(nameof(CustomerCreated.AggregateId))]
    [MapperIgnoreSource(nameof(CustomerCreated.AggregateSequenceNumber))]
    [MapperIgnoreSource(nameof(CustomerCreated.EventId))]
    [MapperIgnoreSource(nameof(CustomerCreated.EventType))]
    [MapperIgnoreSource(nameof(CustomerCreated.EventVersion))]
    [MapperIgnoreSource(nameof(CustomerCreated.TimeStamp))]
    internal static partial CreateCustomerRead ToCreateCustomerRead(this CustomerCreated customerCreated);

    [MapperIgnoreSource(nameof(CustomerUpdated.AggregateId))]
    [MapperIgnoreSource(nameof(CustomerUpdated.AggregateSequenceNumber))]
    [MapperIgnoreSource(nameof(CustomerUpdated.EventType))]
    [MapperIgnoreSource(nameof(CustomerUpdated.EventId))]
    [MapperIgnoreSource(nameof(CustomerUpdated.EventVersion))]
    [MapperIgnoreSource(nameof(CustomerUpdated.TimeStamp))]
    [MapProperty(nameof(CustomerUpdated.Id), nameof(UpdateCustomerRead.CustomerId))]
    [MapProperty(nameof(CustomerUpdated.IdentityId), nameof(UpdateCustomerRead.IdentityId))]
    [MapPropertyFromSource(nameof(UpdateCustomerRead.Id), Use = nameof(MapDefaultGuidId))]
    internal static partial UpdateCustomerRead ToUpdateCustomerRead(this CustomerUpdated customerUpdated);

    [MapProperty(nameof(UpdateCustomerRead.OccurredOn), nameof(CustomerReadModel.Created))]
    [MapperIgnoreTarget(nameof(CustomerReadModel.CreatedBy))]
    [MapperIgnoreSource(nameof(UpdateCustomerRead.InternalCommandId))]
    [MapperIgnoreSource(nameof(UpdateCustomerRead.Type))]
    internal static partial CustomerReadModel ToCustomerReadModel(this UpdateCustomerRead customerUpdated);

    // https://mapperly.riok.app/docs/configuration/mapper/#user-implemented-property-mappings
    // https://mapperly.riok.app/docs/configuration/user-implemented-methods/
    [MapPropertyFromSource(nameof(UpdateCustomer.Id), Use = nameof(MapDefaultLongId))]
    [MapProperty(nameof(UpdateCustomerRequest.Address), nameof(UpdateCustomer.DetailAddress))]
    [MapProperty(nameof(UpdateCustomerRequest.BirthDate), nameof(UpdateCustomer.BirthDate))]
    [MapProperty(nameof(UpdateCustomerRequest.PhoneNumber), nameof(UpdateCustomer.PhoneNumber))]
    [MapProperty(nameof(UpdateCustomerRequest.Email), nameof(UpdateCustomer.Email))]
    internal static partial UpdateCustomer ToUpdateCustomer(this UpdateCustomerRequest updateCustomerRequest);

    private static long MapDefaultLongId(UpdateCustomerRequest request) => 0;

    private static Guid MapDefaultGuidId(CustomerUpdated request) => Guid.NewGuid();
}
