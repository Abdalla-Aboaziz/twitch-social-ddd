using TwitchUniConnect.SharedKernel.Interfaces;

namespace TwitchUniConnect.Social.Application.Interfaces;

public interface IDomainEventDispatcher
{
    Task DispatchAsync(IReadOnlyCollection<IDomainEvent> domainEvents, CancellationToken cancellationToken = default);
}
