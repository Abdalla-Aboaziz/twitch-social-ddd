using Microsoft.EntityFrameworkCore;
using TwitchUniConnect.Social.Application.Interfaces;
using TwitchUniConnect.Social.Domain.Aggregates.ProfileAggregate;

namespace TwitchUniConnect.Social.DAL;

internal sealed class ProfileRepository(SocialDbContext context) : IProfileRepository
{
    public Task<Profile?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        context.Profiles
            .Include("_pendingFriendRequests")
            .SingleOrDefaultAsync(profile => profile.Id == id, cancellationToken);

    public void Add(Profile profile) => context.Profiles.Add(profile);
}
