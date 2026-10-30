using TwitchUniConnect.SharedKernel.Interfaces;
using TwitchUniConnect.SharedKernel.Types;

namespace TwitchUniConnect.Social.Domain.Aggregates.UserAggregate;

public class User : Entity<Guid>, IAggregateRoot
{
    #region Constractors and Properties

    private List<User> _colleagues = new();
    private List<User> _friends = new();
    private List<FriendRequest> _pendingFriendRequests = new();
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

    #region Factories 
    public static User Create(Guid id, string username, UserType type, Name name, EmailAddress email,
         string phone, string password = null, Faculty faculty = null, Class studentclass = null,
         IEnumerable<User> colleagues = null, IEnumerable<User> friends = null,
         IEnumerable<FriendRequest> friendRequests = null)
    {
        if (id == default)
            throw new ArgumentException("Id cannot be a default value", nameof(id));


        var user = new User(id)
        {
            Name = name,
            Username = username,
            Type = type,
            Email = email,
            PhoneNumber = phone,
            Password = password,
            Faculty = faculty
        };
        if (studentclass != null && type != UserType.Student)
            throw new ArgumentException("Only students can be assigned to a class", nameof(studentclass));
        user.Class = studentclass;
        user._colleagues = colleagues != null ? colleagues.ToList() : [];
        user._friends = friends != null ? friends.ToList() : [];
        user._pendingFriendRequests = friendRequests != null ? friendRequests.ToList() : [];

        return user;
    }

    #endregion

    #region puplic methods 
    public void SendFriendRequest(User targetUser)
    {
        var friendRequest = FriendRequest.Create(Id, targetUser.Id);
        targetUser._pendingFriendRequests.Add(friendRequest);
    }

    public void AcceptFriendRequest(User from)
    {
        var friendRequest = _pendingFriendRequests.FirstOrDefault(f => f.From == from.Id);
        if (friendRequest == null)
            throw new InvalidOperationException("Friend request not found");

        _pendingFriendRequests.Remove(friendRequest);
        _friends.Add(from);
    }

    public void RejectFriendRequest(Guid from)
    {
        var friendRequest = _pendingFriendRequests.FirstOrDefault(f => f.From == from);
        if (friendRequest == null)
            throw new InvalidOperationException("Friend request not found");
        _pendingFriendRequests.Remove(friendRequest);
    }
    public void RemoveFriend(User friend)
    {
        ArgumentNullException.ThrowIfNull(friend);
        var existingFriend = _friends.FirstOrDefault(f => f.Id == friend.Id);
        if (existingFriend == null)
            throw new InvalidOperationException("Friend Not Found");
        _friends.Remove(existingFriend);
    }

    public void AddColleague(User user)
    {
        ArgumentNullException.ThrowIfNull(user);
        if (_colleagues.Any(c => c.Id == user.Id))
            throw new InvalidOperationException("User is already a colleague");
        _colleagues.Add(user);
    }
    public void RemoveColleague(User user)
    {
        ArgumentNullException.ThrowIfNull(user);
        var colleague = _colleagues.FirstOrDefault(c => c.Id == user.Id) ?? throw new InvalidOperationException("User is not a colleague");
        _colleagues.Remove(colleague);
    }

    public void SetPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be null or whitespace.", nameof(password));

        Password = password;
    }

    public void ChangePassword(string newPassword)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(newPassword);
        SetPassword(newPassword);
    }
    public void AddFaculty(Faculty faculty)
    {
        ArgumentNullException.ThrowIfNull(faculty);
        if (Faculty != null)
            throw new ArgumentException("the specified Faculty is already assigned to the user ");

        Faculty = faculty;
    }
    public void UpdateFaculty(Faculty faculty)
    {
        ArgumentNullException.ThrowIfNull(faculty);
        Faculty = faculty;
    }

    public void RemoveUserFromFaculty()
    {
        Faculty = null;
    }

    public void AssignStudentToClass(Class studentClass)
    {
        ArgumentNullException.ThrowIfNull(studentClass);
        if (Type != UserType.Student)
            throw new InvalidOperationException("Only students can be assigned to a class");

        Class = studentClass;
    }
    public void RemoveStudentFromClass()
    {
        if (Type != UserType.Student)
            throw new InvalidOperationException("Only students can be removed from a class");
        if (Class == null)
            throw new InvalidOperationException("User is not assigned to a class");
        Class = null;
    }


    #endregion

}