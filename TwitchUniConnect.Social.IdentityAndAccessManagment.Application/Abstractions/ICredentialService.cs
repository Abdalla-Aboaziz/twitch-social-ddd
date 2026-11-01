namespace TwitchUniConnect.Social.IdentityAndAccessManagment.Application.Abstractions;

public interface ICredentialService
{
    string HashPassword(string password);
    bool VerifyPassword(string passwordHash, string password);
}
