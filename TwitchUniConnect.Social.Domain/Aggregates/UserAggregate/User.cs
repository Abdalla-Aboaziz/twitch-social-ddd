using TwitchUniConnect.SharedKernel.Interfaces;
using TwitchUniConnect.SharedKernel.Types;

namespace TwitchUniConnect.Social.Domain.Aggregates.UserAggregate;

public class User : Entity<Guid>, IAggregateRoot
{
    #region Constractors and Properties

    private readonly List<User> _colleagues = new();
    private readonly List<User> _friends = new();
    private readonly List<FriendRequest> _pendingFriendRequests = new();
    private User() : base(Guid.NewGuid()) { }

    private User(Guid id) : base(id)
    {

    }
    public string Username { get; private set; } = null!;
    public string Password { get; private set; } = null!;

    public UserType Type { get; private set; } = null!;
    public Name Name { get; private set; } = null!;
    public Faculty Faculty { get; private set; }

    public Class Class { get; private set; }

    public string PhoneNumber { get; private set; } = default!;
    public EmailAddress Email { get; private set; }

    public IEnumerable<User> Colleagues
    {
        get
        {
            return _colleagues;
        }
    }

    public IEnumerable<User> Friends
    {
        get
        {
            return _friends;
        }
    }
    public IEnumerable<FriendRequest> PendingFriendRequest
    {
        get
        {
            return _pendingFriendRequests;
        }
    }


    #endregion

}