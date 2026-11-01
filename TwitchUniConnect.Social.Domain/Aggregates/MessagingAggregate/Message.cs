using TwitchUniConnect.SharedKernel.Interfaces;
using TwitchUniConnect.SharedKernel.Types;

namespace TwitchUniConnect.Social.Domain.Aggregates.MessagingAggregate;

public sealed class Message : AggregateRoot<Guid>, IAggregateRoot
{
    public const int MaximumContentLength = 4000;

    private Message(Guid id) : base(id)
    {
    }

    public Guid ConversationId { get; private set; }
    public Guid SenderProfileId { get; private set; }
    public string Content { get; private set; } = string.Empty;
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime LastModifiedAtUtc { get; private set; }
    public bool IsDeleted { get; private set; }

    public static Message Create(Guid id, Guid conversationId, Guid senderProfileId, string content)
    {
        ValidateId(id, nameof(id));
        ValidateId(conversationId, nameof(conversationId));
        ValidateId(senderProfileId, nameof(senderProfileId));
        ValidateContent(content);

        var now = DateTime.UtcNow;
        var message = new Message(id)
        {
            ConversationId = conversationId,
            SenderProfileId = senderProfileId,
            Content = content.Trim(),
            CreatedAtUtc = now,
            LastModifiedAtUtc = now
        };
        message.AddDomainEvent(new MessageSentDomainEvent(id, conversationId, senderProfileId, now));
        return message;
    }

    public void Edit(Guid actorProfileId, string content)
    {
        EnsureSender(actorProfileId);
        if (IsDeleted)
            throw new InvalidOperationException("A deleted message cannot be edited.");
        ValidateContent(content);

        Content = content.Trim();
        LastModifiedAtUtc = DateTime.UtcNow;
        AddDomainEvent(new MessageEditedDomainEvent(Id, ConversationId, SenderProfileId, LastModifiedAtUtc));
    }

    public void Delete(Guid actorProfileId)
    {
        EnsureSender(actorProfileId);
        if (IsDeleted)
            throw new InvalidOperationException("The message has already been deleted.");

        IsDeleted = true;
        Content = string.Empty;
        LastModifiedAtUtc = DateTime.UtcNow;
        AddDomainEvent(new MessageDeletedDomainEvent(Id, ConversationId, SenderProfileId, LastModifiedAtUtc));
    }

    private void EnsureSender(Guid actorProfileId)
    {
        ValidateId(actorProfileId, nameof(actorProfileId));
        if (SenderProfileId != actorProfileId)
            throw new InvalidOperationException("Only the sender can modify this message.");
    }

    private static void ValidateId(Guid value, string parameterName)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ID cannot be empty.", parameterName);
    }

    private static void ValidateContent(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Message content cannot be empty.", nameof(content));
        if (content.Length > MaximumContentLength)
            throw new ArgumentException($"Message content cannot exceed {MaximumContentLength} characters.", nameof(content));
    }
}
