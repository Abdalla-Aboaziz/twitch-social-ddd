
using TwitchUniConnect.SharedKernel.Types;

namespace TwitchUniConnect.Social.Domain.Aggregates.PostAggregate;

public class InteractionType : Enumeration
{
    public static readonly InteractionType Like =
        new("Like", 0);

    public static readonly InteractionType Dislike =
        new("Dislike", 1);

    public static readonly InteractionType Love =
        new("Love", 2);

    public static readonly InteractionType Haha =
        new("Haha", 3);

    public static readonly InteractionType Wow =
        new("Wow", 4);

    public static readonly InteractionType Sad =
        new("Sad", 5);

    public static readonly InteractionType Angry =
        new("Angry", 6);

    private InteractionType(string description, int value)
        : base(description, value)
    {
    }
}
