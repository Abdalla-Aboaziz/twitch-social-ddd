using TwitchUniConnect.Social.IdentityAndAccessManagment.Domain.Aggregates.UserAggregate;

namespace TwitchUniConnect.Social.IdentityAndAccessManagment.Application.Abstractions;

public interface IIdentityRepository
{
    Task<User?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken);
    Task<User?> GetUserByNameOrEmailAsync(string normalizedIdentifier, CancellationToken cancellationToken);
    Task<bool> IsUserNameOrEmailTakenAsync(string normalizedUserName, string normalizedEmailAddress, Guid? excludingUserId, CancellationToken cancellationToken);
    Task AddUserAsync(User user, CancellationToken cancellationToken);
    Task<RefreshToken?> GetRefreshTokenByHashAsync(string tokenHash, CancellationToken cancellationToken);
    Task AddRefreshTokenAsync(RefreshToken refreshToken, CancellationToken cancellationToken);
    Task RevokeRefreshTokensForUserAsync(Guid userId, DateTime revokedAtUtc, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
