using TwitchUniConnect.SharedKernel.Types;

namespace TwitchUniConnect.Social.Domain.Aggregates.UserAggregate;

public class Class : Entity<Guid>
{
    #region Constractors and Properties
    private readonly List<User> _students = new();
    private Class(Guid id, DateTime startingYear, DateTime endingYear) : base(id)
    {
        StartingYear = startingYear;
        EndingYear = endingYear;
    }
    public DateTime StartingYear { get; private set; }
    public DateTime EndingYear { get; private set; }
    public Faculty Faculty { get; private set; } = default!;
    public Department Department { get; private set; } = default!;
    public Degree Degree { get; private set; } = default!;

    public IEnumerable<User> Students
    {
        get
        {
            return _students;
        }
    }

    #endregion

    #region Factories
    public static Class Create(
    Guid id,
    DateTime startingYear,
    DateTime endingYear,
    Faculty faculty,
    Department department,
    Degree degree,
    IEnumerable<User>? students = null)
    {
        if (startingYear >= endingYear)
            throw new ArgumentException("Starting year cannot be greater than or equal to ending year.", nameof(startingYear));

        ArgumentNullException.ThrowIfNull(faculty);
        ArgumentNullException.ThrowIfNull(department);

        if (!faculty.HasDepartment(department))
            throw new ArgumentException("Provided department does not belong to the specified faculty.", nameof(department));

        var newClass = new Class(id, startingYear, endingYear)
        {
            Faculty = faculty,
            Department = department,
            Degree = degree
        };

        if (students is not null && students.Any())
        {
            if (!EnsureUsersAreStudents(students))
                throw new ArgumentException("All users in the students list must have Student user type.", nameof(students));

            newClass._students.AddRange(students);
        }

        return newClass;
    }



    #endregion

    #region Private Method

    private static bool EnsureUsersAreStudents(IEnumerable<User> students)
    {
        return students.All(s => s is not null && s.Type == UserType.Student);
    }

    #endregion

    #region Public Methods

    public void AddStudentToClass(User user)
    {
        ArgumentNullException.ThrowIfNull(user);

        if (user.Type != UserType.Student)
            throw new ArgumentException("Only users with Student type can be added to a class.", nameof(user));

        _students.Add(user);
    }

    #endregion



}