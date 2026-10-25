using TwitchUniConnect.SharedKernel.Interfaces;
using TwitchUniConnect.SharedKernel.Types;

namespace TwitchUniConnect.Social.Domain.Aggregates.UserAggreagate;

public class User : Entity<Guid>, IAggregateRoot
{
    #region Constractors and Properties
    private User() : base(Guid.NewGuid()) { }

    private User(Guid id) : base(id)
    {

    }
    public string Username { get; private set; } = null!;
    public string Password { get; private set; } = null!;

    public UserType Type { get; private set; } = null!;
    public Name Name { get; private set; } = null!;

    #endregion
}
