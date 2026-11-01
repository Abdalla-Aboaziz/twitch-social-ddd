using TwitchUniConnect.Social.Application.Interfaces;
using TwitchUniConnect.Social.Domain.Aggregates.PostAggregate;

namespace TwitchUniConnect.Social.Application.Services;

public sealed class CommentService(PostService postService, IUnitOfWork unitOfWork)
{
    public Task<Comment> AddAsync(Guid postId, Guid authorProfileId, string message, CancellationToken cancellationToken = default) =>
        AddToPost(postId, authorProfileId, message, null, cancellationToken);

    public Task<Comment> ReplyAsync(Guid postId, Guid parentCommentId, Guid authorProfileId, string message, CancellationToken cancellationToken = default) =>
        AddToPost(postId, authorProfileId, message, parentCommentId, cancellationToken);

    private async Task<Comment> AddToPost(
        Guid postId, Guid authorProfileId, string message, Guid? parentCommentId, CancellationToken cancellationToken)
    {
        await postService.EnsureProfileExists(authorProfileId, cancellationToken);
        var post = await postService.GetPost(postId, cancellationToken);
        var comment = Comment.Create(Guid.NewGuid(), authorProfileId, message);

        if (parentCommentId.HasValue)
            post.AddReply(parentCommentId.Value, comment);
        else
            post.AddComment(comment);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return comment;
    }
}
