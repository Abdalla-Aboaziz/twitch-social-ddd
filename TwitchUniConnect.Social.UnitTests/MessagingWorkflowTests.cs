using TwitchUniConnect.Social.Application.Interfaces;
using TwitchUniConnect.Social.Application.Services;
using TwitchUniConnect.Social.Domain.Aggregates.MessagingAggregate;
using TwitchUniConnect.Social.Domain.Aggregates.ProfileAggregate;
using Xunit;

namespace TwitchUniConnect.Social.UnitTests;

public sealed class MessagingWorkflowTests
{
    [Fact]
    public async Task Participants_can_create_conversations_send_messages_and_manage_their_own_messages()
    {
        var profiles = new FakeProfileRepository();
        var sender = CreateProfile();
        var recipient = CreateProfile();
        profiles.Add(sender);
        profiles.Add(recipient);

        var conversations = new FakeConversationRepository();
        var messages = new FakeMessageRepository();
        var unitOfWork = new FakeUnitOfWork();
        var service = new MessagingService(conversations, messages, profiles, unitOfWork);

        var conversation = await service.CreateConversationAsync(
            Guid.NewGuid(), sender.Id, [sender.Id, recipient.Id], "Project chat");
        var firstMessage = await service.SendMessageAsync(conversation.Id, sender.Id, "Hello");
        var latestMessage = await service.SendMessageAsync(conversation.Id, recipient.Id, "Hi there");

        await service.EditMessageAsync(conversation.Id, firstMessage.Id, sender.Id, "Hello again");
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.EditMessageAsync(conversation.Id, firstMessage.Id, recipient.Id, "Impersonation"));

        var page = await service.ListMessagesAsync(conversation.Id, sender.Id, page: 1, pageSize: 1);
        Assert.Single(page);
        Assert.Equal(latestMessage.Id, page[0].Id);

        await service.DeleteMessageAsync(conversation.Id, latestMessage.Id, recipient.Id);
        Assert.True(latestMessage.IsDeleted);
        Assert.Empty(latestMessage.Content);
        Assert.True(conversation.LastActivityAtUtc >= firstMessage.CreatedAtUtc);
        Assert.NotEmpty(conversation.DomainEvents);
        Assert.NotEmpty(firstMessage.DomainEvents);
        Assert.True(unitOfWork.SaveCount >= 5);
    }

    [Fact]
    public async Task Non_participants_cannot_read_or_send_messages()
    {
        var profiles = new FakeProfileRepository();
        var creator = CreateProfile();
        var member = CreateProfile();
        var outsider = CreateProfile();
        profiles.Add(creator);
        profiles.Add(member);
        profiles.Add(outsider);

        var conversations = new FakeConversationRepository();
        var service = new MessagingService(conversations, new FakeMessageRepository(), profiles, new FakeUnitOfWork());
        var conversation = await service.CreateConversationAsync(
            Guid.NewGuid(), creator.Id, [creator.Id, member.Id], null);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.SendMessageAsync(conversation.Id, outsider.Id, "Not allowed"));
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.ListMessagesAsync(conversation.Id, outsider.Id));
    }

    private static Profile CreateProfile() => Profile.Create(
        Guid.NewGuid(),
        Name.Create("Social", "Member"),
        Guid.NewGuid(),
        TwitchUniConnect.SharedKernel.Types.EmailAddress.Create(Guid.NewGuid().ToString("N"), "example.com"),
        "+15550000000");

    private sealed class FakeProfileRepository : IProfileRepository
    {
        private readonly Dictionary<Guid, Profile> _profiles = [];

        public Task<Profile?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult(_profiles.GetValueOrDefault(id));

        public void Add(Profile profile) => _profiles.Add(profile.Id, profile);
    }

    private sealed class FakeConversationRepository : IConversationRepository
    {
        private readonly Dictionary<Guid, Conversation> _conversations = [];

        public Task<Conversation?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult(_conversations.GetValueOrDefault(id));

        public Task<IReadOnlyList<Conversation>> ListForParticipantAsync(
            Guid profileId, int skip, int take, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Conversation>>(_conversations.Values
                .Where(conversation => conversation.HasParticipant(profileId))
                .OrderByDescending(conversation => conversation.LastActivityAtUtc)
                .ThenByDescending(conversation => conversation.Id)
                .Skip(skip)
                .Take(take)
                .ToArray());

        public void Add(Conversation conversation) => _conversations.Add(conversation.Id, conversation);
    }

    private sealed class FakeMessageRepository : IMessageRepository
    {
        private readonly Dictionary<Guid, Message> _messages = [];

        public Task<Message?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult(_messages.GetValueOrDefault(id));

        public Task<IReadOnlyList<Message>> ListByConversationAsync(
            Guid conversationId, int skip, int take, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Message>>(_messages.Values
                .Where(message => message.ConversationId == conversationId)
                .OrderByDescending(message => message.CreatedAtUtc)
                .ThenByDescending(message => message.Id)
                .Skip(skip)
                .Take(take)
                .ToArray());

        public void Add(Message message) => _messages.Add(message.Id, message);
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public int SaveCount { get; private set; }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveCount++;
            return Task.FromResult(1);
        }
    }
}
