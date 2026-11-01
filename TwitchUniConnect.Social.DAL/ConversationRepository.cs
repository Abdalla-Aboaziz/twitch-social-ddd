using Microsoft.EntityFrameworkCore;
using TwitchUniConnect.Social.Application.Interfaces;
using TwitchUniConnect.Social.Domain.Aggregates.MessagingAggregate;

namespace TwitchUniConnect.Social.DAL;

internal sealed class ConversationRepository(SocialDbContext context) : IConversationRepository
{
    public Task<Conversation?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        context.Conversations.SingleOrDefaultAsync(conversation => conversation.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Conversation>> ListForParticipantAsync(
        Guid profileId, int skip, int take, CancellationToken cancellationToken = default) =>
        await context.Conversations
            .AsNoTracking()
            .Where(conversation => conversation.Participants.Any(participant => participant.ProfileId == profileId))
            .OrderByDescending(conversation => conversation.LastActivityAtUtc)
            .ThenByDescending(conversation => conversation.Id)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

    public void Add(Conversation conversation) => context.Conversations.Add(conversation);
}
