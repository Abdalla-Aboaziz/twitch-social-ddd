using TwitchUniConnect.Social.Domain.Aggregates.MessagingAggregate;

namespace TwitchUniConnect.Social.Application.Interfaces;

public interface IMessageRepository
{
    Task<Message?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Message>> ListByConversationAsync(
        Guid conversationId, int skip, int take, CancellationToken cancellationToken = default);
    void Add(Message message);
}
