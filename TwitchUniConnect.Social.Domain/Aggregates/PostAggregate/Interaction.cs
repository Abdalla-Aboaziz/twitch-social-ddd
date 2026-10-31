using TwitchUniConnect.SharedKernel.Types;

namespace TwitchUniConnect.Social.Domain.Aggregates.PostAggregate;

public class Interaction : ValueObject
{
    public Interaction()
    {

    }
    public InteractionType Type { get; private set; }
    public Guid AuthorId { get; private set; }

    public static Interaction Create(InteractionType type, Guid authorId)
    {
        if (authorId == Guid.Empty)
        {
            throw new ArgumentException("Author ID cannot be empty.", nameof(authorId));
        }
        return new Interaction
        {
            Type = type,
            AuthorId = authorId
        };
    }

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return Type;
        yield return AuthorId;
    }
}
