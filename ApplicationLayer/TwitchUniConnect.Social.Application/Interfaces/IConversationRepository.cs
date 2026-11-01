using TwitchUniConnect.Social.Domain.Aggregates.MessagingAggregate;

namespace TwitchUniConnect.Social.Application.Interfaces;

public interface IConversationRepository
{
    Task<Conversation?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Conversation>> ListForParticipantAsync(
        Guid profileId, int skip, int take, CancellationToken cancellationToken = default);
    void Add(Conversation conversation);
}
