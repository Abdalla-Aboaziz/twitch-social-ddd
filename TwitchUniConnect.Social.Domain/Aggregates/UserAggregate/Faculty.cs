using TwitchUniConnect.SharedKernel.Types;

namespace TwitchUniConnect.Social.Domain.Aggregates.UserAggregate;

public class Faculty : Entity<Guid>
{
    #region Constructors and Properties 
    private readonly List<Department> _departments = new();


    private Faculty() : base(default) { }

    private Faculty(Guid id, string name, string description) : base(id)
    {
        Name = name;
        Description = description;
    }

    public string Name { get; private set; } = default!;
    public string Description { get; private set; } = default!;


    public IReadOnlyCollection<Department> Departments => _departments.AsReadOnly();

    #endregion

    #region Factories
    public static Faculty Create(Guid id, string name, string description, IEnumerable<Department>? departments = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Faculty name cannot be null or whitespace.", nameof(name));
        }

        var faculty = new Faculty(id, name, description);

        if (departments is not null)
        {
            faculty._departments.AddRange(departments);
        }

        return faculty;
    }
    #endregion

    #region Behaviours

    public void AddDepartment(Department department)
    {
        ArgumentNullException.ThrowIfNull(department);

        if (!_departments.Contains(department))
        {
            _departments.Add(department);
        }
    }

    public void RemoveDepartment(Department department)
    {
        ArgumentNullException.ThrowIfNull(department);
        _departments.Remove(department);
    }

    public void ClearDepartments()
    {
        _departments.Clear();
    }

    public bool HasDepartment(Department department)
    {
        ArgumentNullException.ThrowIfNull(department);
        return _departments.Contains(department);
    }

    public void ChangeName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
        {
            throw new ArgumentException("Faculty name cannot be null or whitespace.", nameof(newName));
        }
        Name = newName;
    }

    public void ChangeDescription(string newDescription)
    {
        Description = newDescription;
    }

    public void EditFaculty(string newName, string newDescription)
    {
        ChangeName(newName);
        ChangeDescription(newDescription);
    }

    #endregion
}