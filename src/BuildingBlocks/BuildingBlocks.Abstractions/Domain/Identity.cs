namespace BuildingBlocks.Abstractions.Domain;

public abstract record Identity<TId>
{
    protected Identity(TId value) => Value = value;
    public TId Value { get; protected set; }

    public static implicit operator TId(Identity<TId> identityId)
        => identityId.Value;

    public override string ToString()
    {
        return IdAsString();
    }

    public string IdAsString()
    {
        return $"{GetType().Name} [Id={Value}]";
    }
}

public abstract record Identity : Identity<long>
{
    protected Identity(long value) : base(value)
    {
    }
}
