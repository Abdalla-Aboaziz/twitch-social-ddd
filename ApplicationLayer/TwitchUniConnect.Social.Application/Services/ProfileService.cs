using TwitchUniConnect.SharedKernel.Types;
using TwitchUniConnect.Social.Application.Exceptions;
using TwitchUniConnect.Social.Application.Interfaces;
using TwitchUniConnect.Social.Domain.Aggregates.ProfileAggregate;

namespace TwitchUniConnect.Social.Application.Services;

public sealed class ProfileService(IProfileRepository profiles, IUnitOfWork unitOfWork)
{
    public async Task<Profile> CreateAsync(
        Guid id, Guid userId, string firstName, string lastName, string emailName, string emailDomain, string phoneNumber,
        CancellationToken cancellationToken = default)
    {
        var profile = Profile.Create(
            id,
            Name.Create(firstName, lastName),
            userId,
            EmailAddress.Create(emailName, emailDomain),
            phoneNumber);

        profiles.Add(profile);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return profile;
    }

    public async Task<Profile> GetAsync(Guid id, CancellationToken cancellationToken = default) =>
        await profiles.FindByIdAsync(id, cancellationToken)
        ?? throw new NotFoundException($"Profile '{id}' was not found.");
}
