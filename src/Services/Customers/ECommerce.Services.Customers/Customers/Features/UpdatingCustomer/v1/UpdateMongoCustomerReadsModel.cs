using Ardalis.GuardClauses;
using AutoMapper;
using BuildingBlocks.Abstractions.CQRS.Commands;
using BuildingBlocks.Core.CQRS.Commands;
using ECommerce.Services.Customers.Customers.Models.Reads;
using ECommerce.Services.Customers.Shared.Data;
using MongoDB.Driver;

namespace ECommerce.Services.Customers.Customers.Features.UpdatingCustomer.v1;

public record UpdateMongoCustomerReadsModel : InternalCommand
{
    public new Guid Id { get; init; }
    public long CustomerId { get; init; }
    public Guid IdentityId { get; init; }
    public string Email { get; init; } = null!;
    public string FirstName { get; init; } = null!;
    public string LastName { get; init; } = null!;
    public string FullName { get; init; } = null!;
    public string? Country { get; init; }
    public string? City { get; init; }
    public string? DetailAddress { get; init; }
    public string? Nationality { get; init; }
    public DateTime? BirthDate { get; init; }
    public string? PhoneNumber { get; init; }
}

internal class UpdateMongoCustomerReadsModelHandler : ICommandHandler<UpdateMongoCustomerReadsModel>
{
    private readonly CustomersReadDbContext _customersReadDbContext;
    private readonly IMapper _mapper;

    public UpdateMongoCustomerReadsModelHandler(CustomersReadDbContext customersReadDbContext, IMapper mapper)
    {
        _customersReadDbContext = customersReadDbContext;
        _mapper = mapper;
    }

    public async Task<Unit> Handle(UpdateMongoCustomerReadsModel command, CancellationToken cancellationToken)
    {
        Guard.Against.Null(command, nameof(command));

        var filterDefinition =
            Builders<CustomerReadModel>.Filter
                .Eq(x => x.CustomerId, command.CustomerId);

        var updateDefinition =
            Builders<CustomerReadModel>.Update
                .Set(x => x.Email, command.Email)
                .Set(x => x.Country, command.Country)
                .Set(x => x.City, command.City)
                .Set(x => x.DetailAddress, command.DetailAddress)
                .Set(x => x.IdentityId, command.IdentityId)
                .Set(x => x.CustomerId, command.CustomerId)
                .Set(x => x.Nationality, command.Nationality)
                .Set(x => x.FirstName, command.FirstName)
                .Set(x => x.LastName, command.LastName)
                .Set(x => x.FullName, command.FullName)
                .Set(x => x.PhoneNumber, command.PhoneNumber)
                .Set(x => x.BirthDate, command.BirthDate);

        await _customersReadDbContext.Customers.UpdateOneAsync(
            filterDefinition,
            updateDefinition,
            new UpdateOptions(),
            cancellationToken);

        return Unit.Value;
    }
}
