using TwitchUniConnect.SharedKernel.Types;

namespace TwitchUniConnect.Social.Domain.Aggregates.UserAggregate;

public class FriendRequest : ValueObject
{

    private FriendRequest()
    {

    }

    public Guid From { get; private set; }
    public Guid To { get; private set; }
    public DateTime DateSent { get; private set; }

    public static FriendRequest Create(Guid from, Guid to)
    {
        if (from == Guid.Empty)
        {
            throw new ArgumentException("From user ID cannot be empty.", nameof(from));
        }
        if (to == Guid.Empty)
        {
            throw new ArgumentException("To user ID cannot be empty.", nameof(to));
        }
        return new FriendRequest
        {
            From = from,
            To = to,
            DateSent = DateTime.UtcNow
        };
    }

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return From;
        yield return To;

    }
}
