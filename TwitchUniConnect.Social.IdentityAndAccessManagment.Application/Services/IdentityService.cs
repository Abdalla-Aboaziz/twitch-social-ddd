using TwitchUniConnect.Social.IdentityAndAccessManagment.Application.Abstractions;
using TwitchUniConnect.Social.IdentityAndAccessManagment.Application.Contracts;
using TwitchUniConnect.Social.IdentityAndAccessManagment.Domain.Aggregates.UserAggregate;

namespace TwitchUniConnect.Social.IdentityAndAccessManagment.Application.Services;

public sealed class IdentityService(
    IIdentityRepository repository,
    ICredentialService credentials,
    ITokenService tokens)
{
    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        ValidatePassword(request.Password);
        var normalizedUserName = Normalize(request.UserName);
        var normalizedEmail = Normalize(request.EmailAddress);
        if (await repository.IsUserNameOrEmailTakenAsync(normalizedUserName, normalizedEmail, null, cancellationToken))
            throw new InvalidOperationException("The username or email address is already registered.");

        var user = User.Create(Guid.NewGuid(), request.UserName, credentials.HashPassword(request.Password), request.EmailAddress, request.PhoneNumber);
        await repository.AddUserAsync(user, cancellationToken);

        var refresh = tokens.CreateRefreshToken();
        await repository.AddRefreshTokenAsync(CreateRefreshToken(user.Id, refresh), cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return CreateResponse(user, refresh);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await repository.GetUserByNameOrEmailAsync(Normalize(request.Identifier), cancellationToken);
        if (user is null || !user.IsActive || !credentials.VerifyPassword(user.PasswordHash, request.Password))
            throw new UnauthorizedAccessException("Invalid credentials.");

        var refresh = tokens.CreateRefreshToken();
        await repository.AddRefreshTokenAsync(CreateRefreshToken(user.Id, refresh), cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return CreateResponse(user, refresh);
    }

    public async Task<AuthResponse> RefreshAsync(RefreshRequest request, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var presentedHash = tokens.HashRefreshToken(request.RefreshToken);
        var storedToken = await repository.GetRefreshTokenByHashAsync(presentedHash, cancellationToken);
        if (storedToken is null || !storedToken.IsActiveAt(now))
            throw new UnauthorizedAccessException("Invalid or expired refresh token.");

        var user = await repository.GetUserByIdAsync(storedToken.UserId, cancellationToken);
        if (user is null || !user.IsActive)
            throw new UnauthorizedAccessException("The account is inactive.");

        var refresh = tokens.CreateRefreshToken();
        storedToken.Revoke(now, refresh.TokenHash);
        await repository.AddRefreshTokenAsync(CreateRefreshToken(user.Id, refresh), cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return CreateResponse(user, refresh);
    }

    public async Task RevokeAsync(RefreshRequest request, CancellationToken cancellationToken = default)
    {
        var storedToken = await repository.GetRefreshTokenByHashAsync(tokens.HashRefreshToken(request.RefreshToken), cancellationToken);
        if (storedToken is null)
            return;
        if (!storedToken.RevokedAtUtc.HasValue)
            storedToken.Revoke(DateTime.UtcNow);
        await repository.SaveChangesAsync(cancellationToken);
    }

    public async Task<AccountResponse> GetAccountAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await GetActiveUserAsync(userId, cancellationToken);
        return ToAccountResponse(user);
    }

    public async Task<AccountResponse> UpdateAccountAsync(Guid userId, UpdateAccountRequest request, CancellationToken cancellationToken = default)
    {
        var user = await GetActiveUserAsync(userId, cancellationToken);
        if (await repository.IsUserNameOrEmailTakenAsync(Normalize(request.UserName), Normalize(request.EmailAddress), userId, cancellationToken))
            throw new InvalidOperationException("The username or email address is already registered.");
        user.UpdateAccountDetails(request.UserName, request.EmailAddress, request.PhoneNumber);
        await repository.SaveChangesAsync(cancellationToken);
        return ToAccountResponse(user);
    }

    public async Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken cancellationToken = default)
    {
        ValidatePassword(request.NewPassword);
        var user = await GetActiveUserAsync(userId, cancellationToken);
        if (!credentials.VerifyPassword(user.PasswordHash, request.CurrentPassword))
            throw new UnauthorizedAccessException("Current password is incorrect.");

        user.UpdatePasswordHash(credentials.HashPassword(request.NewPassword));
        await repository.RevokeRefreshTokensForUserAsync(userId, DateTime.UtcNow, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
    }

    public async Task DeactivateAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await GetActiveUserAsync(userId, cancellationToken);
        user.Deactivate();
        await repository.RevokeRefreshTokensForUserAsync(userId, DateTime.UtcNow, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
    }

    private async Task<User> GetActiveUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await repository.GetUserByIdAsync(userId, cancellationToken);
        return user is { IsActive: true } ? user : throw new UnauthorizedAccessException("Account is unavailable.");
    }

    private RefreshToken CreateRefreshToken(Guid userId, GeneratedRefreshToken token) =>
        RefreshToken.Create(Guid.NewGuid(), userId, token.TokenHash, DateTime.UtcNow, token.ExpiresAtUtc);

    private AuthResponse CreateResponse(User user, GeneratedRefreshToken refresh)
    {
        var access = tokens.CreateAccessToken(user);
        return new AuthResponse(user.Id, user.UserName, user.EmailAddress, access.Token, access.ExpiresAtUtc, refresh.Token, refresh.ExpiresAtUtc);
    }

    private static AccountResponse ToAccountResponse(User user) =>
        new(user.Id, user.UserName, user.EmailAddress, user.PhoneNumber, user.CreatedAtUtc, user.UpdatedAtUtc);

    private static string Normalize(string value) => (value ?? string.Empty).Trim().ToUpperInvariant();

    private static void ValidatePassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < 12 || password.Length > 256)
            throw new ArgumentException("Password must contain between 12 and 256 characters.", nameof(password));
    }
}
