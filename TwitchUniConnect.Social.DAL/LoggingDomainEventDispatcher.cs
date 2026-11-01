using Microsoft.Extensions.Logging;
using TwitchUniConnect.SharedKernel.Interfaces;
using TwitchUniConnect.Social.Application.Interfaces;

namespace TwitchUniConnect.Social.DAL;

internal sealed class LoggingDomainEventDispatcher(ILogger<LoggingDomainEventDispatcher> logger) : IDomainEventDispatcher
{
    public Task DispatchAsync(IReadOnlyCollection<IDomainEvent> domainEvents, CancellationToken cancellationToken = default)
    {
        foreach (var domainEvent in domainEvents)
            logger.LogInformation("Dispatched domain event {DomainEventType} at {OccurredOnUtc}",
                domainEvent.GetType().Name, domainEvent.OccurredOnUtc);

        return Task.CompletedTask;
    }
}
