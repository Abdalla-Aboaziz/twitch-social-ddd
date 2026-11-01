using TwitchUniConnect.Social.Application.Exceptions;
using TwitchUniConnect.Social.Application.Interfaces;
using TwitchUniConnect.Social.Domain.Aggregates.MessagingAggregate;

namespace TwitchUniConnect.Social.Application.Services;

public sealed class MessagingService(
    IConversationRepository conversations,
    IMessageRepository messages,
    IProfileRepository profiles,
    IUnitOfWork unitOfWork)
{
    public async Task<Conversation> CreateConversationAsync(
        Guid id,
        Guid creatorProfileId,
        IEnumerable<Guid> participantProfileIds,
        string? title,
        CancellationToken cancellationToken = default)
    {
        var conversation = Conversation.Create(id, creatorProfileId, participantProfileIds, title);
        foreach (var participantId in conversation.ParticipantProfileIds)
        {
            _ = await profiles.FindByIdAsync(participantId, cancellationToken)
                ?? throw new NotFoundException($"Profile '{participantId}' was not found.");
        }

        conversations.Add(conversation);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return conversation;
    }

    public async Task<Conversation> GetConversationAsync(
        Guid conversationId, Guid requesterProfileId, CancellationToken cancellationToken = default) =>
        await GetAuthorizedConversationAsync(conversationId, requesterProfileId, cancellationToken);

    public async Task<IReadOnlyList<Conversation>> ListConversationsAsync(
        Guid profileId, int page = 1, int pageSize = 50, CancellationToken cancellationToken = default)
    {
        if (profileId == Guid.Empty)
            throw new ArgumentException("Profile ID cannot be empty.", nameof(profileId));
        _ = await profiles.FindByIdAsync(profileId, cancellationToken)
            ?? throw new NotFoundException($"Profile '{profileId}' was not found.");
        var skip = GetSkip(page, pageSize);
        return await conversations.ListForParticipantAsync(profileId, skip, pageSize, cancellationToken);
    }

    public async Task<Message> SendMessageAsync(
        Guid conversationId,
        Guid senderProfileId,
        string content,
        CancellationToken cancellationToken = default)
    {
        var conversation = await GetAuthorizedConversationAsync(conversationId, senderProfileId, cancellationToken);
        var message = Message.Create(Guid.NewGuid(), conversationId, senderProfileId, content);
        conversation.RecordMessageActivity(senderProfileId, message.CreatedAtUtc);
        messages.Add(message);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return message;
    }

    public async Task<IReadOnlyList<Message>> ListMessagesAsync(
        Guid conversationId,
        Guid requesterProfileId,
        int page = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        _ = await GetAuthorizedConversationAsync(conversationId, requesterProfileId, cancellationToken);
        return await messages.ListByConversationAsync(conversationId, GetSkip(page, pageSize), pageSize, cancellationToken);
    }

    public async Task<Message> EditMessageAsync(
        Guid conversationId,
        Guid messageId,
        Guid actorProfileId,
        string content,
        CancellationToken cancellationToken = default)
    {
        _ = await GetAuthorizedConversationAsync(conversationId, actorProfileId, cancellationToken);
        var message = await GetMessageAsync(conversationId, messageId, cancellationToken);
        message.Edit(actorProfileId, content);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return message;
    }

    public async Task<Message> DeleteMessageAsync(
        Guid conversationId,
        Guid messageId,
        Guid actorProfileId,
        CancellationToken cancellationToken = default)
    {
        _ = await GetAuthorizedConversationAsync(conversationId, actorProfileId, cancellationToken);
        var message = await GetMessageAsync(conversationId, messageId, cancellationToken);
        message.Delete(actorProfileId);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return message;
    }

    private async Task<Conversation> GetAuthorizedConversationAsync(
        Guid conversationId, Guid requesterProfileId, CancellationToken cancellationToken)
    {
        var conversation = await conversations.FindByIdAsync(conversationId, cancellationToken)
            ?? throw new NotFoundException($"Conversation '{conversationId}' was not found.");
        conversation.EnsureParticipant(requesterProfileId);
        return conversation;
    }

    private async Task<Message> GetMessageAsync(
        Guid conversationId, Guid messageId, CancellationToken cancellationToken)
    {
        var message = await messages.FindByIdAsync(messageId, cancellationToken)
            ?? throw new NotFoundException($"Message '{messageId}' was not found.");
        if (message.ConversationId != conversationId)
            throw new NotFoundException($"Message '{messageId}' was not found in conversation '{conversationId}'.");
        return message;
    }

    private static int GetSkip(int page, int pageSize)
    {
        if (page < 1)
            throw new ArgumentOutOfRangeException(nameof(page), "Page must be greater than zero.");
        if (pageSize is < 1 or > 100)
            throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be between 1 and 100.");

        var skip = (long)(page - 1) * pageSize;
        if (skip > int.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(page), "Requested page is too large.");
        return (int)skip;
    }
}
