using TwitchUniConnect.SharedKernel.Types;

namespace TwitchUniConnect.Social.Domain.Aggregates.UserAggreagate;

/// <summary>
/// instead of using Enum 
/// </summary>
public class UserType : Enumeration
{
    public static readonly UserType Student = new UserType("Student", 1);
    public static readonly UserType Teacher = new UserType("Teacher", 2);
    public static readonly UserType Staff = new UserType("Staff", 3);

    private UserType(string description, int value) : base(description, value)
    {
    }
}
/* 
 public enum UserType
{
    Student = 1,
    Teacher = 2,
    Staff = 3

}
 */
