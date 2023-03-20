using BuildingBlocks.Core.CQRS.Events.Internal;
using ECommerce.Services.Customers.Customers.Models;

namespace ECommerce.Services.Customers.Customers.Features.UpdatingCustomer.v1.Events.Domain;

public record CustomerUpdated(Customer Customer) : DomainEvent;
