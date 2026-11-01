using TwitchUniConnect.SharedKernel.Interfaces;
using TwitchUniConnect.SharedKernel.Types;

namespace TwitchUniConnect.Social.Domain.Aggregates.MessagingAggregate;

public sealed class Conversation : AggregateRoot<Guid>, IAggregateRoot
{
    public const int MaximumParticipants = 100;
    private readonly List<ConversationParticipant> _participants = [];

    private Conversation(Guid id) : base(id)
    {
    }

    public Guid CreatorProfileId { get; private set; }
    public string? Title { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime LastActivityAtUtc { get; private set; }
    public IReadOnlyCollection<ConversationParticipant> Participants => _participants.AsReadOnly();
    public IReadOnlyCollection<Guid> ParticipantProfileIds => _participants.Select(participant => participant.ProfileId).ToArray();
    public bool IsGroup => _participants.Count > 2;

    public static Conversation Create(
        Guid id,
        Guid creatorProfileId,
        IEnumerable<Guid> participantProfileIds,
        string? title = null)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Conversation ID cannot be empty.", nameof(id));
        if (creatorProfileId == Guid.Empty)
            throw new ArgumentException("Creator profile ID cannot be empty.", nameof(creatorProfileId));
        ArgumentNullException.ThrowIfNull(participantProfileIds);

        var participants = participantProfileIds.Distinct().ToList();
        if (participants.Any(participantId => participantId == Guid.Empty))
            throw new ArgumentException("Participant profile IDs cannot be empty.", nameof(participantProfileIds));
        if (participants.Count is < 2 or > MaximumParticipants)
            throw new ArgumentException($"A conversation must have between 2 and {MaximumParticipants} participants.", nameof(participantProfileIds));
        if (!participants.Contains(creatorProfileId))
            throw new ArgumentException("The creator must be a conversation participant.", nameof(participantProfileIds));
        ValidateTitle(title);

        var now = DateTime.UtcNow;
        var conversation = new Conversation(id)
        {
            CreatorProfileId = creatorProfileId,
            Title = string.IsNullOrWhiteSpace(title) ? null : title.Trim(),
            CreatedAtUtc = now,
            LastActivityAtUtc = now
        };
        foreach (var participantId in participants)
            conversation._participants.Add(ConversationParticipant.Create(participantId, now));
        conversation.AddDomainEvent(new ConversationCreatedDomainEvent(id, creatorProfileId, now));
        return conversation;
    }

    public bool HasParticipant(Guid profileId) => _participants.Any(participant => participant.ProfileId == profileId);

    public void AddParticipant(Guid profileId)
    {
        if (profileId == Guid.Empty)
            throw new ArgumentException("Participant profile ID cannot be empty.", nameof(profileId));
        if (HasParticipant(profileId))
            throw new InvalidOperationException("The profile is already a participant.");
        if (_participants.Count >= MaximumParticipants)
            throw new InvalidOperationException($"A conversation cannot have more than {MaximumParticipants} participants.");

        var now = DateTime.UtcNow;
        _participants.Add(ConversationParticipant.Create(profileId, now));
        LastActivityAtUtc = now;
        AddDomainEvent(new ConversationParticipantAddedDomainEvent(Id, profileId, now));
    }

    public void RemoveParticipant(Guid profileId)
    {
        var participant = _participants.FirstOrDefault(value => value.ProfileId == profileId);
        if (participant is null)
            throw new InvalidOperationException("The profile is not a conversation participant.");
        if (_participants.Count <= 2)
            throw new InvalidOperationException("A conversation must retain at least two participants.");

        _participants.Remove(participant);
        var now = DateTime.UtcNow;
        LastActivityAtUtc = now;
        AddDomainEvent(new ConversationParticipantRemovedDomainEvent(Id, profileId, now));
    }

    public void RecordMessageActivity(Guid senderProfileId, DateTime sentAtUtc)
    {
        EnsureParticipant(senderProfileId);
        if (sentAtUtc.Kind != DateTimeKind.Utc)
            throw new ArgumentException("Message time must be UTC.", nameof(sentAtUtc));
        if (sentAtUtc < CreatedAtUtc)
            throw new ArgumentException("Message time cannot be earlier than conversation creation.", nameof(sentAtUtc));

        if (sentAtUtc > LastActivityAtUtc)
            LastActivityAtUtc = sentAtUtc;
    }

    public void EnsureParticipant(Guid profileId)
    {
        if (profileId == Guid.Empty)
            throw new ArgumentException("Profile ID cannot be empty.", nameof(profileId));
        if (!HasParticipant(profileId))
            throw new InvalidOperationException("Only conversation participants can access this conversation.");
    }

    private static void ValidateTitle(string? title)
    {
        if (title?.Length > 120)
            throw new ArgumentException("Conversation title cannot exceed 120 characters.", nameof(title));
    }
}
