namespace TwitchUniConnect.Social.Api.Contracts;

public sealed record CreateProfileRequest(Guid UserId, string FirstName, string LastName, string EmailName, string EmailDomain, string PhoneNumber);
public sealed record CreatePostRequest(Guid AuthorProfileId, string Text);
public sealed record EditPostTextRequest(Guid AuthorProfileId, string Text);
public sealed record CreateCommentRequest(Guid AuthorProfileId, string Message);
public sealed record CreateReplyRequest(Guid AuthorProfileId, string Message);
public sealed record CreateInteractionRequest(Guid AuthorProfileId, string Type);
public sealed record CreateConversationRequest(Guid CreatorProfileId, IReadOnlyCollection<Guid> ParticipantProfileIds, string? Title);
public sealed record SendMessageRequest(Guid SenderProfileId, string Content);
public sealed record EditMessageRequest(Guid ActorProfileId, string Content);

public sealed record ProfileResponse(Guid Id, Guid UserId, string FirstName, string LastName, string Email, string PhoneNumber, IReadOnlyCollection<Guid> Friends);
public sealed record PostResponse(Guid Id, Guid AuthorProfileId, string Type, string Text, DateTime? DateCreated, DateTime? LastModified, IReadOnlyCollection<Uri> MediaUrls, int CommentCount, int InteractionCount);
public sealed record CommentResponse(Guid Id, Guid AuthorProfileId, string Message, DateTime? DateCreated, IReadOnlyCollection<Guid> Replies);
public sealed record ConversationResponse(Guid Id, Guid CreatorProfileId, string? Title, bool IsGroup, DateTime CreatedAtUtc, DateTime LastActivityAtUtc, IReadOnlyCollection<Guid> ParticipantProfileIds);
public sealed record MessageResponse(Guid Id, Guid ConversationId, Guid SenderProfileId, string? Content, bool IsDeleted, DateTime CreatedAtUtc, DateTime LastModifiedAtUtc);
