using TwitchUniConnect.SharedKernel.Types;

namespace TwitchUniConnect.Social.Domain.Aggregates.UserAggregate;

public class Degree : Enumeration
{
    public static readonly Degree Bachelor = new Degree("Bachelor", 0);
    public static readonly Degree Master = new Degree("Master", 1);
    public static readonly Degree Doctorate = new Degree("Doctorate", 2);
    public static readonly Degree PostDoctorate = new Degree("Post-Doctorate research", 3);


    private Degree(string description, int value) : base(description, value)
    {

    }
}
