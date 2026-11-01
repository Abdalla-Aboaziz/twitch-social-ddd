using TwitchUniConnect.SharedKernel.Types;

namespace TwitchUniConnect.Social.Domain.Aggregates.MessagingAggregate;

public sealed class ConversationParticipant : ValueObject
{
    private ConversationParticipant()
    {
    }

    public Guid ProfileId { get; private set; }
    public DateTime JoinedAtUtc { get; private set; }

    internal static ConversationParticipant Create(Guid profileId, DateTime joinedAtUtc) => new()
    {
        ProfileId = profileId,
        JoinedAtUtc = joinedAtUtc
    };

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return ProfileId;
        yield return JoinedAtUtc;
    }
}
