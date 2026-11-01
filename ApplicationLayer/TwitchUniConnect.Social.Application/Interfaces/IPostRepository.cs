using TwitchUniConnect.Social.Domain.Aggregates.PostAggregate;

namespace TwitchUniConnect.Social.Application.Interfaces;

public interface IPostRepository
{
    Task<Post?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default);
    void Add(Post post);
}
