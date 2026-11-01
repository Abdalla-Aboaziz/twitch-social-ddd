using TwitchUniConnect.SharedKernel.Interfaces;
using TwitchUniConnect.SharedKernel.Types;

namespace TwitchUniConnect.Social.Domain.Aggregates.PostAggregate;

public class Post : AggregateRoot<Guid>, IAggregateRoot
{
    #region Constructors and properties

    private readonly List<Uri> _mediaUrls = [];
    private readonly List<Comment> _comments = [];
    private readonly List<Interaction> _interactions = [];
    private Post(Guid id) : base(id)
    {
    }

    public PostType Type { get; private set; }
    public string Text { get; private set; }
    public Guid AuthorProfileId { get; private set; }
    public IReadOnlyCollection<Uri> MediaUrl => _mediaUrls.AsReadOnly();
    public DateTime? DateCreated { get; private set; }
    public DateTime? LastModified { get; private set; }
    public IReadOnlyCollection<Comment> Comments => _comments.AsReadOnly();

    public IReadOnlyCollection<Interaction> Interactions => _interactions.AsReadOnly();

    public int NumberOfComments
    {
        get { return _comments.Count; }
    }

    public int NumberOfInteractions
    {
        get { return _interactions.Count; }
    }
    #endregion

    #region Factories
    public static Post Create(Guid id, PostType type, string text, Guid authorProfileId,
        DateTime? dateCreated = null, DateTime? lastModified = null,
        IEnumerable<Uri>? mediaUrls = null, IEnumerable<Comment>? comments = null,
        IEnumerable<Interaction>? interactions = null)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("The identifier cannot be empty.", nameof(id));
        if (authorProfileId == Guid.Empty)
            throw new ArgumentException("Author profile ID cannot be empty.", nameof(authorProfileId));
        ArgumentNullException.ThrowIfNull(type);
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("Post text cannot be empty.", nameof(text));
        if (text.Length > 5000)
            throw new ArgumentException("Post text cannot exceed 5000 characters.", nameof(text));

        var createdAt = dateCreated ?? DateTime.UtcNow;
        var modifiedAt = lastModified ?? createdAt;
        if (modifiedAt < createdAt)
            throw new ArgumentException("Last modified cannot be earlier than date created.", nameof(lastModified));

        var post = new Post(id)
        {
            Type = type,
            Text = text,
            AuthorProfileId = authorProfileId,
            DateCreated = createdAt,
            LastModified = modifiedAt
        };

        if (mediaUrls is not null)
        {
            foreach (var url in mediaUrls)
            {
                ValidateMediaUrl(url);
                post._mediaUrls.Add(url);
            }
        }
        if (post._mediaUrls.Count > 10)
            throw new ArgumentException("A post cannot contain more than 10 media URLs.", nameof(mediaUrls));
        if (comments is not null)
        {
            foreach (var comment in comments)
            {
                ArgumentNullException.ThrowIfNull(comment);
                post._comments.Add(comment);
            }
        }
        if (interactions is not null)
        {
            foreach (var interaction in interactions)
            {
                ArgumentNullException.ThrowIfNull(interaction);
                post._interactions.Add(interaction);
            }
        }

        post.AddDomainEvent(new PostCreatedDomainEvent(id, authorProfileId, createdAt));

        return post;

    }
    #endregion

    #region Public methods
    public void AddMediaUrl(Uri url)
    {
        ValidateMediaUrl(url);
        if (_mediaUrls.Count >= 10)
            throw new InvalidOperationException("A post cannot contain more than 10 media URLs.");
        if (_mediaUrls.Contains(url))
            throw new InvalidOperationException("This media URL has already been added.");
        _mediaUrls.Add(url);
        LastModified = DateTime.UtcNow;
        AddDomainEvent(new MediaUrlAddedDomainEvent(Id, url, LastModified.Value));
        // TO DO: Fire media added event
    }

    public void RemoveMediaUrl(Uri url)
    {
        var toRemove = _mediaUrls.FirstOrDefault(u => u == url);

        if (toRemove != null)
        {
            _mediaUrls.Remove(toRemove);
            LastModified = DateTime.UtcNow;
            AddDomainEvent(new MediaUrlRemovedDomainEvent(Id, toRemove, LastModified.Value));
        }
    }

    public void AddComment(Comment comment)
    {
        ArgumentNullException.ThrowIfNull(comment);
        if (_comments.Any(existing => existing.Id == comment.Id))
            throw new InvalidOperationException("This comment has already been added to the post.");
        _comments.Add(comment);
        LastModified = DateTime.UtcNow;
        AddDomainEvent(new CommentAddedDomainEvent(Id, comment.Id, comment.AuthorProfileId, null, LastModified.Value));
    }

    public void AddReply(Guid parentCommentId, Comment reply)
    {
        ArgumentNullException.ThrowIfNull(reply);
        var parent = _comments.FirstOrDefault(comment => comment.Id == parentCommentId)
            ?? throw new InvalidOperationException("Parent comment was not found on this post.");
        if (_comments.Any(comment => comment.Id == reply.Id))
            throw new InvalidOperationException("This comment has already been added to the post.");

        parent.AddReply(reply.Id);
        _comments.Add(reply);
        LastModified = DateTime.UtcNow;
        AddDomainEvent(new CommentAddedDomainEvent(Id, reply.Id, reply.AuthorProfileId, parentCommentId, LastModified.Value));
    }

    public void RemoveComment(Guid commentId)
    {
        var toRemove = _comments.FirstOrDefault(c => c.Id == commentId);

        if (!(toRemove is null))
        {
            _comments.Remove(toRemove);
            LastModified = DateTime.UtcNow;
            // TO DO: Fire comment remeoved event
        }
    }

    public void AddInteraction(Interaction interaction)
    {
        ArgumentNullException.ThrowIfNull(interaction);
        var existingInteraction = _interactions.FirstOrDefault(existing => existing.AuthorId == interaction.AuthorId);
        if (existingInteraction is not null)
            _interactions.Remove(existingInteraction);
        _interactions.Add(interaction);
        LastModified = DateTime.UtcNow;
        AddDomainEvent(new InteractionAddedDomainEvent(Id, interaction.AuthorId, interaction.Type, LastModified.Value));
    }

    public void RemoveInteraction(Interaction interaction)
    {
        var toRemove = _interactions.FirstOrDefault(i => i == interaction);
        if (toRemove != null)
        {
            _interactions.Remove(toRemove);
            LastModified = DateTime.UtcNow;
            AddDomainEvent(new InteractionRemovedDomainEvent(Id, toRemove.AuthorId, toRemove.Type, LastModified.Value));
        }
    }

    public void EditPostText(string newText)
    {
        if (string.IsNullOrWhiteSpace(newText))
            throw new ArgumentException("Post text cannot be empty.", nameof(newText));
        if (newText.Length > 5000)
            throw new ArgumentException("Post text cannot exceed 5000 characters.", nameof(newText));

        Text = newText;
        LastModified = DateTime.UtcNow;
        AddDomainEvent(new PostTextEditedDomainEvent(Id, LastModified.Value));
    }

    private static void ValidateMediaUrl(Uri url)
    {
        ArgumentNullException.ThrowIfNull(url);
        if (!url.IsAbsoluteUri)
            throw new ArgumentException("Media URL must be absolute.", nameof(url));
    }
    #endregion
}