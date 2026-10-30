using TwitchUniConnect.SharedKernel.Interfaces;
using TwitchUniConnect.SharedKernel.Types;

namespace TwitchUniConnect.Social.Domain.Aggregates.PostAggregate;

public class Post : Entity<Guid>, IAggregateRoot
{
    #region Constructors and Properties

    private Post(Guid id) : base(id)
    {

    }

    #endregion
}
