using TwitchUniConnect.Social.IdentityAndAccessManagment.Domain.Aggregates.UserAggregate;

namespace TwitchUniConnect.Social.IdentityAndAccessManagment.Application.Abstractions;

public sealed record IssuedAccessToken(string Token, DateTime ExpiresAtUtc);

public sealed record GeneratedRefreshToken(string Token, string TokenHash, DateTime ExpiresAtUtc);

public interface ITokenService
{
    IssuedAccessToken CreateAccessToken(User user);
    GeneratedRefreshToken CreateRefreshToken();
    string HashRefreshToken(string token);
}
