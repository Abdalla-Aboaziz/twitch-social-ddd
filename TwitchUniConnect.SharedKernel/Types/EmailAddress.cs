namespace TwitchUniConnect.SharedKernel.Types;

public class EmailAddress : ValueObject
{
    public string EmailName { get; private set; } = default!;
    public string Domain { get; private set; } = default!;

    public static EmailAddress Create(string emailName, string domain)
    {


        return new EmailAddress
        {
            EmailName = emailName,
            Domain = domain
        };
    }

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return EmailName;
        yield return Domain;
    }

    public override string ToString()
    {
        return $"{EmailName}@{Domain}";
    }
}
