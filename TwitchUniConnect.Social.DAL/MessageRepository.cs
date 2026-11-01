using Microsoft.EntityFrameworkCore;
using TwitchUniConnect.Social.Application.Interfaces;
using TwitchUniConnect.Social.Domain.Aggregates.MessagingAggregate;

namespace TwitchUniConnect.Social.DAL;

internal sealed class MessageRepository(SocialDbContext context) : IMessageRepository
{
    public Task<Message?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        context.Messages.SingleOrDefaultAsync(message => message.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Message>> ListByConversationAsync(
        Guid conversationId, int skip, int take, CancellationToken cancellationToken = default) =>
        await context.Messages
            .AsNoTracking()
            .Where(message => message.ConversationId == conversationId)
            .OrderByDescending(message => message.CreatedAtUtc)
            .ThenByDescending(message => message.Id)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

    public void Add(Message message) => context.Messages.Add(message);
}
