using TwitchUniConnect.SharedKernel.Interfaces;
using TwitchUniConnect.SharedKernel.Types;

namespace TwitchUniConnect.Social.Domain.Aggregates.ProfileAggregate;

public class Profile : AggregateRoot<Guid>, IAggregateRoot
{
    #region Constractors and Properties


    private readonly List<Guid> _friends = [];
    private readonly List<FriendRequest> _pendingFriendRequests = [];

    private Profile(Guid id) : base(id)
    {

    }
    public Guid UserId { get; private set; }
    public Name Name { get; private set; } = null!;
    public string PhoneNumber { get; private set; } = default!;
    public EmailAddress Email { get; private set; } = null!;




    public IReadOnlyCollection<Guid> Friends => _friends.AsReadOnly();
    public IReadOnlyCollection<FriendRequest> PendingFriendRequest => _pendingFriendRequests.AsReadOnly();

    #endregion

    #region Factories 
    public static Profile Create(Guid id, Name name, Guid userId, EmailAddress email, string phone)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Id cannot be a default value", nameof(id));
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty.", nameof(userId));

        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(email);
        if (string.IsNullOrWhiteSpace(phone))
            throw new ArgumentException("Phone number cannot be empty.", nameof(phone));

        var profile = new Profile(id)
        {
            Name = name,
            Email = email,
            PhoneNumber = phone,
            UserId = userId

        };
        profile.AddDomainEvent(new ProfileCreatedDomainEvent(id, DateTime.UtcNow));
        return profile;
    }

    #endregion

    #region puplic methods 
    public void SendFriendRequest(Profile targetUser)
    {
        ArgumentNullException.ThrowIfNull(targetUser);
        if (Id == targetUser.Id)
            throw new InvalidOperationException("A profile cannot send a friend request to itself.");
        if (_friends.Contains(targetUser.Id) || targetUser._friends.Contains(Id))
            throw new InvalidOperationException("These profiles are already friends.");
        if (_pendingFriendRequests.Any(request => request.From == targetUser.Id))
            throw new InvalidOperationException("This profile already has a friend request from the target profile.");
        if (targetUser._pendingFriendRequests.Any(request => request.From == Id))
            throw new InvalidOperationException("A friend request has already been sent.");

        targetUser._pendingFriendRequests.Add(FriendRequest.Create(Id, targetUser.Id));
        targetUser.AddDomainEvent(new FriendRequestSentDomainEvent(Id, targetUser.Id, DateTime.UtcNow));
    }

    public void AcceptFriendRequest(Guid from)
    {
        if (from == Guid.Empty)
            throw new ArgumentException("Sender profile ID cannot be empty.", nameof(from));

        var friendRequest = _pendingFriendRequests.FirstOrDefault(f => f.From == from);
        if (friendRequest == null)
            throw new InvalidOperationException("Friend request not found");

        _pendingFriendRequests.Remove(friendRequest);
        if (!_friends.Contains(from))
            _friends.Add(from);
        AddDomainEvent(new FriendRequestAcceptedDomainEvent(from, Id, DateTime.UtcNow));
        AddDomainEvent(new FriendshipCreatedDomainEvent(Id, from, DateTime.UtcNow));
    }

    public void RejectFriendRequest(Guid from)
    {
        if (from == Guid.Empty)
            throw new ArgumentException("Sender profile ID cannot be empty.", nameof(from));

        var friendRequest = _pendingFriendRequests.FirstOrDefault(f => f.From == from);
        if (friendRequest == null)
            throw new InvalidOperationException("Friend request not found");
        _pendingFriendRequests.Remove(friendRequest);
        AddDomainEvent(new FriendRequestRejectedDomainEvent(from, Id, DateTime.UtcNow));
    }

    public void AddFriend(Guid friendId)
    {
        if (friendId == Guid.Empty)
            throw new ArgumentException("Friend profile ID cannot be empty.", nameof(friendId));
        if (friendId == Id)
            throw new InvalidOperationException("A profile cannot be friends with itself.");
        if (!_friends.Contains(friendId))
        {
            _friends.Add(friendId);
            AddDomainEvent(new FriendshipCreatedDomainEvent(Id, friendId, DateTime.UtcNow));
        }
    }

    public void RemoveFriend(Guid friendId)
    {
        if (friendId == Guid.Empty)
            throw new ArgumentException("Friend profile ID cannot be empty.", nameof(friendId));

        var existingFriend = _friends.FirstOrDefault(f => f == friendId);
        if (existingFriend == default)
            throw new InvalidOperationException("Friend Not Found");
        _friends.Remove(existingFriend);
    }




    #endregion

}