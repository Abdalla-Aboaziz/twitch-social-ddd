using TwitchUniConnect.SharedKernel.Interfaces;

namespace TwitchUniConnect.Social.Domain.Aggregates.PostAggregate;

public sealed record PostCreatedDomainEvent(Guid PostId, Guid AuthorProfileId, DateTime OccurredOnUtc) : IDomainEvent;

public sealed record PostTextEditedDomainEvent(Guid PostId, DateTime OccurredOnUtc) : IDomainEvent;

public sealed record CommentAddedDomainEvent(Guid PostId, Guid CommentId, Guid AuthorProfileId, Guid? ParentCommentId, DateTime OccurredOnUtc) : IDomainEvent;

public sealed record InteractionAddedDomainEvent(Guid PostId, Guid AuthorProfileId, InteractionType Type, DateTime OccurredOnUtc) : IDomainEvent;

public sealed record InteractionRemovedDomainEvent(Guid PostId, Guid AuthorProfileId, InteractionType Type, DateTime OccurredOnUtc) : IDomainEvent;

public sealed record MediaUrlAddedDomainEvent(Guid PostId, Uri MediaUrl, DateTime OccurredOnUtc) : IDomainEvent;

public sealed record MediaUrlRemovedDomainEvent(Guid PostId, Uri MediaUrl, DateTime OccurredOnUtc) : IDomainEvent;
