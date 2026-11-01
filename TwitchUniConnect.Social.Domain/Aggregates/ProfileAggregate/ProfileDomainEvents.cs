using TwitchUniConnect.SharedKernel.Interfaces;

namespace TwitchUniConnect.Social.Domain.Aggregates.ProfileAggregate;

public sealed record ProfileCreatedDomainEvent(Guid ProfileId, DateTime OccurredOnUtc) : IDomainEvent;

public sealed record FriendRequestSentDomainEvent(Guid SenderProfileId, Guid RecipientProfileId, DateTime OccurredOnUtc) : IDomainEvent;

public sealed record FriendRequestAcceptedDomainEvent(Guid SenderProfileId, Guid RecipientProfileId, DateTime OccurredOnUtc) : IDomainEvent;

public sealed record FriendRequestRejectedDomainEvent(Guid SenderProfileId, Guid RecipientProfileId, DateTime OccurredOnUtc) : IDomainEvent;

public sealed record FriendshipCreatedDomainEvent(Guid ProfileId, Guid FriendProfileId, DateTime OccurredOnUtc) : IDomainEvent;
