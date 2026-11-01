using TwitchUniConnect.Social.Application.Exceptions;
using TwitchUniConnect.Social.Application.Interfaces;
using TwitchUniConnect.Social.Domain.Aggregates.ProfileAggregate;

namespace TwitchUniConnect.Social.Application.Services;

public sealed class FriendshipService(IProfileRepository profiles, IUnitOfWork unitOfWork)
{
    public async Task SendRequestAsync(Guid senderId, Guid recipientId, CancellationToken cancellationToken = default)
    {
        var sender = await GetProfile(senderId, cancellationToken);
        var recipient = await GetProfile(recipientId, cancellationToken);
        sender.SendFriendRequest(recipient);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task RespondToRequestAsync(
        Guid recipientId, Guid senderId, bool accept, CancellationToken cancellationToken = default)
    {
        var recipient = await GetProfile(recipientId, cancellationToken);
        if (accept)
        {
            var sender = await GetProfile(senderId, cancellationToken);
            recipient.AcceptFriendRequest(senderId);
            sender.AddFriend(recipientId);
        }
        else
        {
            recipient.RejectFriendRequest(senderId);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<Profile> GetProfile(Guid id, CancellationToken cancellationToken) =>
        await profiles.FindByIdAsync(id, cancellationToken)
        ?? throw new NotFoundException($"Profile '{id}' was not found.");
}
