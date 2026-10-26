using TwitchUniConnect.SharedKernel.Types;

namespace TwitchUniConnect.Social.Domain.Aggregates.UserAggregate;

public class Department : ValueObject
{
    #region Constractors and Properties
    private Department(string name, string description)
    {
        Name = name;
        Description = description;
    }
    private Department() { }

    public string Name { get; private set; } = default!;
    public string Description { get; private set; } = default;



    #endregion

    #region Public Methods 


    public Department ChangeDepartmentName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
        {
            throw new ArgumentException("Department name cannot be null or whitespace.", nameof(newName));
        }
        return Create(newName, Description);
    }
    public Department ChangeDepartmentDescription(string description)
    {

        return Create(Name, description);
    }

    public Department EditDepartment(string newName, string newDescription)
    {
        if (string.IsNullOrWhiteSpace(newName))
        {
            throw new ArgumentException("Department name cannot be null or whitespace.", nameof(newName));
        }
        return Create(newName, newDescription);
    }
    #endregion

    #region Factories
    public static Department Create(string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Department name cannot be null or whitespace.", nameof(name));
        }

        return new Department(name, description);

    }

    #endregion
    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return Name;
        yield return Description;
    }


}
