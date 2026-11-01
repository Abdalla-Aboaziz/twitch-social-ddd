using TwitchUniConnect.SharedKernel.Interfaces;

namespace TwitchUniConnect.Social.Domain.Aggregates.MessagingAggregate;

public sealed record ConversationCreatedDomainEvent(Guid ConversationId, Guid CreatorProfileId, DateTime OccurredOnUtc) : IDomainEvent;

public sealed record ConversationParticipantAddedDomainEvent(Guid ConversationId, Guid ProfileId, DateTime OccurredOnUtc) : IDomainEvent;

public sealed record ConversationParticipantRemovedDomainEvent(Guid ConversationId, Guid ProfileId, DateTime OccurredOnUtc) : IDomainEvent;

public sealed record MessageSentDomainEvent(Guid MessageId, Guid ConversationId, Guid SenderProfileId, DateTime OccurredOnUtc) : IDomainEvent;

public sealed record MessageEditedDomainEvent(Guid MessageId, Guid ConversationId, Guid SenderProfileId, DateTime OccurredOnUtc) : IDomainEvent;

public sealed record MessageDeletedDomainEvent(Guid MessageId, Guid ConversationId, Guid SenderProfileId, DateTime OccurredOnUtc) : IDomainEvent;
