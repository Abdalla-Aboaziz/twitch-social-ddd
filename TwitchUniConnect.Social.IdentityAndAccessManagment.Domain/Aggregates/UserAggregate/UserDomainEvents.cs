using TwitchUniConnect.SharedKernel.Interfaces;

namespace TwitchUniConnect.Social.IdentityAndAccessManagment.Domain.Aggregates.UserAggregate;

public sealed record UserRegisteredDomainEvent(Guid UserId, string EmailAddress, DateTime OccurredOnUtc) : IDomainEvent;

public sealed record UserAccountUpdatedDomainEvent(Guid UserId, DateTime OccurredOnUtc) : IDomainEvent;

public sealed record UserPasswordChangedDomainEvent(Guid UserId, DateTime OccurredOnUtc) : IDomainEvent;

public sealed record UserDeactivatedDomainEvent(Guid UserId, DateTime OccurredOnUtc) : IDomainEvent;

public sealed record RefreshTokenCreatedDomainEvent(Guid TokenId, Guid UserId, DateTime OccurredOnUtc) : IDomainEvent;

public sealed record RefreshTokenRevokedDomainEvent(Guid TokenId, Guid UserId, DateTime OccurredOnUtc) : IDomainEvent;
