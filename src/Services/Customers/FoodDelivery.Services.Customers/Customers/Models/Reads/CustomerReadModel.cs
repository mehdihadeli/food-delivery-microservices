using BuildingBlocks.Abstractions.Domain;

namespace FoodDelivery.Services.Customers.Customers.Models.Reads;

public record CustomerReadModel : IEntity<Guid>
{
    public Guid Id { get; init; }
    public required long CustomerId { get; init; }
    public required Guid IdentityId { get; init; }
    public required string Email { get; init; } = null!;
    public required string FirstName { get; init; } = null!;
    public required string LastName { get; init; } = null!;
    public required string FullName { get; init; } = null!;
    public required string PhoneNumber { get; init; }
    public string? Country { get; init; }
    public string? City { get; init; }
    public string? DetailAddress { get; init; }
    public string? Nationality { get; init; }
    public DateTime? BirthDate { get; init; }
    public required DateTime Created { get; init; }
    public int? CreatedBy { get; init; }
}
