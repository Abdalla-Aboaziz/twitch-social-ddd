using TwitchUniConnect.SharedKernel.Interfaces;
using TwitchUniConnect.SharedKernel.Types;
using System.Net.Mail;

namespace TwitchUniConnect.Social.IdentityAndAccessManagment.Domain.Aggregates.UserAggregate;

public class User : AggregateRoot<Guid>, IAggregateRoot
{
    #region Fields , constractors , properties

    private User(Guid id) : base(id)
    {

    }

    public string UserName { get; private set; } = default!;
    public string NormalizedUserName { get; private set; } = default!;
    public string EmailAddress { get; private set; } = default!;
    public string NormalizedEmailAddress { get; private set; } = default!;
    public string PasswordHash { get; private set; } = default!;
    public string PhoneNumber { get; private set; } = default!;
    public bool IsActive { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }


    #endregion

    #region Factories
    public static User Create(Guid id, string userName, string passwordHash, string emailAddress, string phoneNumber)
    {
        if (id == default)
            throw new ArgumentException("Id cannot be a default value", nameof(id));

        ValidateUserName(userName);
        ValidatePasswordHash(passwordHash);
        ValidateEmailAddress(emailAddress);
        ValidatePhoneNumber(phoneNumber);

        var now = DateTime.UtcNow;
        var user = new User(id)
        {
            UserName = userName.Trim(),
            NormalizedUserName = Normalize(userName),
            EmailAddress = emailAddress.Trim(),
            NormalizedEmailAddress = Normalize(emailAddress),
            PasswordHash = passwordHash,
            PhoneNumber = phoneNumber.Trim(),
            IsActive = true,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };
        user.AddDomainEvent(new UserRegisteredDomainEvent(id, user.EmailAddress, now));
        return user;
    }

    #endregion

    #region public methods 

    public void UpdateUserName(string userName)
    {
        ValidateUserName(userName);
        UserName = userName.Trim();
        NormalizedUserName = Normalize(userName);
        UpdatedAtUtc = DateTime.UtcNow;
        AddDomainEvent(new UserAccountUpdatedDomainEvent(Id, UpdatedAtUtc));
    }

    public void UpdatePasswordHash(string passwordHash)
    {
        ValidatePasswordHash(passwordHash);
        PasswordHash = passwordHash;
        UpdatedAtUtc = DateTime.UtcNow;
        AddDomainEvent(new UserPasswordChangedDomainEvent(Id, UpdatedAtUtc));
    }

    public void UpdateEmailAddress(string emailAddress)
    {
        ValidateEmailAddress(emailAddress);
        EmailAddress = emailAddress.Trim();
        NormalizedEmailAddress = Normalize(emailAddress);
        UpdatedAtUtc = DateTime.UtcNow;
        AddDomainEvent(new UserAccountUpdatedDomainEvent(Id, UpdatedAtUtc));
    }

    public void UpdatePhoneNumber(string phoneNumber)
    {
        ValidatePhoneNumber(phoneNumber);
        PhoneNumber = phoneNumber.Trim();
        UpdatedAtUtc = DateTime.UtcNow;
        AddDomainEvent(new UserAccountUpdatedDomainEvent(Id, UpdatedAtUtc));
    }

    public void UpdateAccountDetails(string userName, string emailAddress, string phoneNumber)
    {
        ValidateUserName(userName);
        ValidateEmailAddress(emailAddress);
        ValidatePhoneNumber(phoneNumber);
        UserName = userName.Trim();
        NormalizedUserName = Normalize(userName);
        EmailAddress = emailAddress.Trim();
        NormalizedEmailAddress = Normalize(emailAddress);
        PhoneNumber = phoneNumber.Trim();
        UpdatedAtUtc = DateTime.UtcNow;
        AddDomainEvent(new UserAccountUpdatedDomainEvent(Id, UpdatedAtUtc));
    }

    public void Deactivate()
    {
        if (!IsActive)
            throw new InvalidOperationException("The account is already inactive.");
        IsActive = false;
        UpdatedAtUtc = DateTime.UtcNow;
        AddDomainEvent(new UserDeactivatedDomainEvent(Id, UpdatedAtUtc));
    }

    #endregion

    #region private validation methods

    private static void ValidateUserName(string userName)
    {
        if (string.IsNullOrWhiteSpace(userName) || userName.Trim().Length is < 3 or > 50)
            throw new ArgumentException("UserName must contain between 3 and 50 characters.", nameof(userName));
    }

    private static void ValidatePasswordHash(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash cannot be empty.", nameof(passwordHash));
    }

    private static void ValidateEmailAddress(string emailAddress)
    {
        if (string.IsNullOrWhiteSpace(emailAddress) || emailAddress.Length > 254 ||
            !MailAddress.TryCreate(emailAddress.Trim(), out var address) ||
            !string.Equals(address.Address, emailAddress.Trim(), StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("EmailAddress is not in a valid format.", nameof(emailAddress));

    }

    private static void ValidatePhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber) || phoneNumber.Trim().Length > 40)
            throw new ArgumentException("PhoneNumber must contain between 1 and 40 characters.", nameof(phoneNumber));
    }

    private static string Normalize(string value) => value.Trim().ToUpperInvariant();

    #endregion

}
