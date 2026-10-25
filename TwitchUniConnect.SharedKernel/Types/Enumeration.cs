namespace TwitchUniConnect.SharedKernel.Types;

public abstract class Enumeration : IComparable, IEquatable<Enumeration>
{
    public Enumeration(string description, int value)
    {
        Description = description;
        Value = value;
    }
    public int Value { get; private set; }
    public string Description { get; private set; }


    public bool Equals(Enumeration? other)
    {
        if (other == null)
            return false;

        var typeMatches = GetType().Equals(other.GetType());
        var valueMatches = Value.Equals(other.Value);

        return typeMatches && valueMatches;
    }
    public override bool Equals(object? obj)
    {
        var other = obj as Enumeration;
        if (other == null)
            return false;

        return Equals(other);
    }
    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }
    public static bool operator ==(Enumeration? left, Enumeration? right)
    {
        if (left is null && right is null) return true;
        if (left is null || right is null) return false;
        return left.Equals(right);
    }
    public static bool operator !=(Enumeration? left, Enumeration? right)
    {
        return !(left == right);
    }

    public override string ToString()
    {
        return Description;
    }

    public int CompareTo(object? obj)
    {
        return Value.CompareTo(((Enumeration)obj!).Value);
    }
}
