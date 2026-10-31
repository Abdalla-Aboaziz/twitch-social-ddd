using TwitchUniConnect.SharedKernel.Interfaces;
using TwitchUniConnect.SharedKernel.Types;

namespace TwitchUniConnect.Social.Domain.Aggregates.PostAggregate;

public class Post : Entity<Guid>, IAggregateRoot
{
    #region Constructors and Properties
    private readonly List<Uri> _mediaUrls;
    private readonly List<Comment> _comments;
    private readonly List<Interaction> _interactions;
    private Post(Guid id) : base(id)
    {

    }
    public PostType Type { get; private set; } = null!;
    public string Text { get; private set; } = default!;
    public Guid AuthorId { get; private set; }
    public IEnumerable<Uri> MediaUrls
    {
        get { return _mediaUrls; }
    }


    public DateTime? DateCreated { get; private set; }
    public DateTime? LastModified { get; private set; }

    public IEnumerable<Comment> Comments
    {
        get { return _comments; }
    }
    public IEnumerable<Interaction> Interactions
    {
        get { return _interactions; }
    }
    public int NumberOfComments
    {
        get { return _comments.Count; }
    }

    public int NumberOfInteractions
    {
        get { return _interactions.Count; }
    }
    public void DeleteComment(Guid commentId)
    {
        var commentToDelete = _comments.FirstOrDefault(c => c.Id == commentId);
        if (commentToDelete == null)
        {
            throw new ArgumentException($"Comment with ID {commentId} not found.");
        }
        _comments.Remove(commentToDelete);
    }

    #endregion
}
