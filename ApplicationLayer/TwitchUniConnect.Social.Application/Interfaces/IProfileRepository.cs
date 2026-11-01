using TwitchUniConnect.Social.Domain.Aggregates.ProfileAggregate;

namespace TwitchUniConnect.Social.Application.Interfaces;

public interface IProfileRepository
{
    Task<Profile?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default);
    void Add(Profile profile);
}
