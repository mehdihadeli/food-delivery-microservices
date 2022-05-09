namespace BuildingBlocks.Core.Domain;

/// <summary>
/// Enumeration class based on https://lostechies.com/jimmybogard/2008/08/12/enumeration-classes/
/// </summary>
public abstract class Enumeration : IComparable
{
    public int Id { get; }

    public string Name { get; }

    protected Enumeration(int id, string name)
    {
        Id = id;
        Name = name;
    }

    public override bool Equals(object obj)
    {
        if (!(obj is Enumeration otherValue))
            return false;

        var typeMatches = GetType() == obj.GetType();
        var valueMatches = Id.Equals(otherValue.Id);

        return typeMatches && valueMatches;
    }

    public override int GetHashCode() => Id.GetHashCode();

    public override string ToString() => Name;

    public int CompareTo(object obj)
        => Id.CompareTo(((Enumeration)obj).Id);
}
