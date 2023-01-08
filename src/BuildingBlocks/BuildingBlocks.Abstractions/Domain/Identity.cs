namespace BuildingBlocks.Abstractions.Domain;

public abstract record Identity<TId>
{
    public TId Value { get; init; } = default!;

    public static implicit operator TId(Identity<TId> identityId)
        => identityId.Value;

    public override string ToString()
    {
        return IdAsString();
    }

    public string IdAsString()
    {
        return $"{GetType().Name} [InternalCommandId={Value}]";
    }
}

public abstract record Identity : Identity<long>;
