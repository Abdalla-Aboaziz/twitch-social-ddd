
namespace TwitchUniConnect.SharedKernel.Types;

public abstract class Entity<TId> : IEquatable<Entity<TId>>
    where TId : notnull
{
    protected Entity(TId id)
    {
        if (!IsValidId(id))
            throw new ArgumentException("Invalid ID", nameof(id));

        Id = id;
    }

    public TId Id { get; }

    public bool Equals(Entity<TId>? other)
    {
        return other is not null &&
               EqualityComparer<TId>.Default.Equals(Id, other.Id);
    }

    public override bool Equals(object? obj)
    {
        return obj is Entity<TId> other && Equals(other);
    }

    public override int GetHashCode()
    {
        return EqualityComparer<TId>.Default.GetHashCode(Id);
    }

    public static bool operator ==(
        Entity<TId>? lhs,
        Entity<TId>? rhs)
    {
        if (ReferenceEquals(lhs, rhs))
            return true;

        if (lhs is null || rhs is null)
            return false;

        return lhs.Equals(rhs);
    }

    public static bool operator !=(
        Entity<TId>? lhs,
        Entity<TId>? rhs)
    {
        return !(lhs == rhs);
    }

    private static bool IsValidId(TId id)
    {
        return id is int || id is Guid;
    }
}
