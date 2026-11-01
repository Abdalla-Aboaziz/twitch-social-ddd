using Microsoft.EntityFrameworkCore;
using TwitchUniConnect.SharedKernel.Interfaces;
using TwitchUniConnect.Social.DAL;
using Xunit;

namespace TwitchUniConnect.Social.UnitTests;

public sealed class SocialPersistenceModelTests
{
    [Fact]
    public void Sql_server_model_is_valid_without_connecting_to_a_database()
    {
        var options = new DbContextOptionsBuilder<SocialDbContext>()
            .UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=ModelValidation;Trusted_Connection=True")
            .Options;
        using var context = new SocialDbContext(options, new NullDomainEventDispatcher());

        Assert.Contains("Profiles", context.Model.GetEntityTypes().Select(entity => entity.GetTableName()));
        Assert.Contains("Posts", context.Model.GetEntityTypes().Select(entity => entity.GetTableName()));
        Assert.Contains("Comments", context.Model.GetEntityTypes().Select(entity => entity.GetTableName()));
        Assert.Contains("Conversations", context.Model.GetEntityTypes().Select(entity => entity.GetTableName()));
        Assert.Contains("ConversationParticipants", context.Model.GetEntityTypes().Select(entity => entity.GetTableName()));
        Assert.Contains("Messages", context.Model.GetEntityTypes().Select(entity => entity.GetTableName()));

        var inboxQuery = context.Conversations
            .Where(conversation => conversation.Participants.Any(participant => participant.ProfileId == Guid.NewGuid()))
            .ToQueryString();
        Assert.Contains("ConversationParticipants", inboxQuery);
    }

    private sealed class NullDomainEventDispatcher : TwitchUniConnect.Social.Application.Interfaces.IDomainEventDispatcher
    {
        public Task DispatchAsync(IReadOnlyCollection<IDomainEvent> domainEvents, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }
}
