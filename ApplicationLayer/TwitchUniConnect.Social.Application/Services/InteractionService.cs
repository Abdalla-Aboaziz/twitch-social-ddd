using TwitchUniConnect.Social.Application.Interfaces;
using TwitchUniConnect.Social.Domain.Aggregates.PostAggregate;

namespace TwitchUniConnect.Social.Application.Services;

public sealed class InteractionService(PostService postService, IUnitOfWork unitOfWork)
{
    public async Task<Post> AddAsync(
        Guid postId, Guid authorProfileId, InteractionType type, CancellationToken cancellationToken = default)
    {
        await postService.EnsureProfileExists(authorProfileId, cancellationToken);
        var post = await postService.GetPost(postId, cancellationToken);
        post.AddInteraction(Interaction.Create(type, authorProfileId));
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return post;
    }

    public async Task<Post> RemoveAsync(
        Guid postId, Guid authorProfileId, InteractionType type, CancellationToken cancellationToken = default)
    {
        var post = await postService.GetPost(postId, cancellationToken);
        post.RemoveInteraction(Interaction.Create(type, authorProfileId));
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return post;
    }
}
