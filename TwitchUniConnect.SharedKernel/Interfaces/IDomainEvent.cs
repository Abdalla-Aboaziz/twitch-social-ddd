namespace TwitchUniConnect.SharedKernel.Interfaces;

public interface IDomainEvent
{
    DateTime OccurredOnUtc { get; }
}
