
using TwitchUniConnect.SharedKernel.Types;

namespace TwitchUniConnect.Social.Domain.Aggregates.PostAggregate;

public class Comment : Entity<Guid>
{
    private const int MaxMessageLength = 2000;
    private List<Guid> _replies;

    #region Constructors and Properties

    private Comment(Guid id) : base(id)
    {
    }

    public Guid AuthorId { get; private set; }

    public string Message { get; private set; } = default!;

    public DateTime? DateCreated { get; private set; }

    public DateTime? LastModified { get; private set; }
    public IEnumerable<Guid> Replies
    {
        get
        {
            return _replies;
        }
    }

    #endregion

    #region Factory

    public static Comment Create(
        Guid id,
        Guid authorId,
        string message,
        IEnumerable<Guid> replies = null
        )
    {
        if (id == Guid.Empty)
            throw new ArgumentException(
                "Comment ID cannot be empty.", nameof(id));

        if (authorId == Guid.Empty)
            throw new ArgumentException(
                "Author ID cannot be empty.", nameof(authorId));

        ValidateMessage(message);

        var now = DateTime.UtcNow;

        return new Comment(id)
        {
            AuthorId = authorId,
            Message = message,
            DateCreated = now,
            LastModified = now,
            _replies = replies != null ? replies.ToList() : new List<Guid>()
        };
    }

    #endregion

    #region Business Methods

    public void EditComment(string newMessage)
    {
        ValidateMessage(newMessage);

        Message = newMessage;
        LastModified = DateTime.UtcNow;
    }

    public void AddReply(Guid commentId)
    {
        if (commentId == Guid.Empty)
            throw new ArgumentException(
                "Comment ID cannot be empty.", nameof(commentId));
        _replies.Add(commentId);
        LastModified = DateTime.UtcNow;
    }
    public void DeleteReply(Guid commentId)
    {
        if (commentId == Guid.Empty)
            throw new ArgumentException(
                "Comment ID cannot be empty.", nameof(commentId));
        _replies.Remove(commentId);
        LastModified = DateTime.UtcNow;
    }



    private static void ValidateMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException(
                "Comment message cannot be empty.",
                nameof(message));

        if (message.Length > MaxMessageLength)
            throw new ArgumentException(
                $"Comment cannot exceed {MaxMessageLength} characters.",
                nameof(message));
    }

    #endregion
}
