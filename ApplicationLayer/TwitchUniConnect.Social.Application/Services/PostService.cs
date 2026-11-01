using TwitchUniConnect.Social.Application.Exceptions;
using TwitchUniConnect.Social.Application.Interfaces;
using TwitchUniConnect.Social.Domain.Aggregates.PostAggregate;
using TwitchUniConnect.Social.Domain.Aggregates.ProfileAggregate;

namespace TwitchUniConnect.Social.Application.Services;

public sealed class PostService(IPostRepository posts, IProfileRepository profiles, IUnitOfWork unitOfWork)
{
    public async Task<Post> CreateAsync(Guid id, Guid authorProfileId, string text, CancellationToken cancellationToken = default)
    {
        await EnsureProfileExists(authorProfileId, cancellationToken);
        var post = Post.Create(id, PostType.Text, text, authorProfileId);
        posts.Add(post);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return post;
    }

    public Task<Post> GetAsync(Guid postId, CancellationToken cancellationToken = default) =>
        GetPost(postId, cancellationToken);

    public async Task<Post> EditTextAsync(Guid postId, Guid authorProfileId, string text, CancellationToken cancellationToken = default)
    {
        var post = await GetPost(postId, cancellationToken);
        if (post.AuthorProfileId != authorProfileId)
            throw new InvalidOperationException("Only the post author can edit it.");

        post.EditPostText(text);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return post;
    }

    internal async Task<Post> GetPost(Guid id, CancellationToken cancellationToken) =>
        await posts.FindByIdAsync(id, cancellationToken)
        ?? throw new NotFoundException($"Post '{id}' was not found.");

    internal async Task EnsureProfileExists(Guid id, CancellationToken cancellationToken)
    {
        _ = await profiles.FindByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Profile '{id}' was not found.");
    }
}
