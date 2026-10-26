using TwitchUniConnect.SharedKernel.Types;

namespace TwitchUniConnect.Social.Domain.Aggregates.UserAggregate;

public class Name : ValueObject
{
    #region Constructors and Properties
    private Name()
    {

    }
    private Name(string firstName, string lastName)
    {
        FirstName = firstName;
        LastName = lastName;
        Initials = FirstName[0].ToString().ToUpperInvariant() + LastName[0].ToString().ToUpperInvariant();
    }

    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string Initials { get; private set; }

    #endregion

    #region Factories 

    public static Name Create(string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(firstName))
        {
            throw new ArgumentException("First name cannot be null or whitespace.", nameof(firstName));
        }

        if (string.IsNullOrWhiteSpace(lastName))
        {
            throw new ArgumentException("Last name cannot be null or whitespace.", nameof(lastName));
        }

        return new Name(firstName, lastName);
    }



    #endregion

    #region Public Methods

    public Name ChangeFirstName(string newFirstName)
    {
        return Create(newFirstName, LastName);
    }
    public Name ChangeLastName(string newLastName)
    {
        return Create(FirstName, newLastName);
    }


    #endregion
    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return FirstName;
        yield return LastName;
    }
}
