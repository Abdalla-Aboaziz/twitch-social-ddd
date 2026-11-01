using Microsoft.EntityFrameworkCore;
using TwitchUniConnect.Social.Application.Interfaces;
using TwitchUniConnect.Social.Domain.Aggregates.PostAggregate;

namespace TwitchUniConnect.Social.DAL;

internal sealed class PostRepository(SocialDbContext context) : IPostRepository
{
    public Task<Post?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        context.Posts
            .Include(post => post.Comments)
            .Include("_interactions")
            .SingleOrDefaultAsync(post => post.Id == id, cancellationToken);

    public void Add(Post post) => context.Posts.Add(post);
}
