using TwitchUniConnect.SharedKernel.Interfaces;
using TwitchUniConnect.SharedKernel.Types;

namespace TwitchUniConnect.Social.IdentityAndAccessManagment.Domain.Aggregates.UserAggregate;

public sealed class RefreshToken : AggregateRoot<Guid>, IAggregateRoot
{
    private RefreshToken(Guid id) : base(id)
    {
    }

    public Guid UserId { get; private set; }
    public string TokenHash { get; private set; } = default!;
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime ExpiresAtUtc { get; private set; }
    public DateTime? RevokedAtUtc { get; private set; }
    public string? ReplacedByTokenHash { get; private set; }

    public static RefreshToken Create(
        Guid id, Guid userId, string tokenHash, DateTime createdAtUtc, DateTime expiresAtUtc)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Refresh token ID cannot be empty.", nameof(id));
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty.", nameof(userId));
        if (string.IsNullOrWhiteSpace(tokenHash) || tokenHash.Length > 128)
            throw new ArgumentException("Token hash is invalid.", nameof(tokenHash));
        if (createdAtUtc.Kind != DateTimeKind.Utc || expiresAtUtc.Kind != DateTimeKind.Utc)
            throw new ArgumentException("Refresh token dates must use UTC.");
        if (expiresAtUtc <= createdAtUtc)
            throw new ArgumentException("Refresh token expiration must follow its creation time.", nameof(expiresAtUtc));

        var token = new RefreshToken(id)
        {
            UserId = userId,
            TokenHash = tokenHash,
            CreatedAtUtc = createdAtUtc,
            ExpiresAtUtc = expiresAtUtc
        };
        token.AddDomainEvent(new RefreshTokenCreatedDomainEvent(id, userId, createdAtUtc));
        return token;
    }

    public bool IsActiveAt(DateTime nowUtc) =>
        nowUtc.Kind == DateTimeKind.Utc && RevokedAtUtc is null && ExpiresAtUtc > nowUtc;

    public void Revoke(DateTime nowUtc, string? replacedByTokenHash = null)
    {
        if (nowUtc.Kind != DateTimeKind.Utc)
            throw new ArgumentException("Revocation date must use UTC.", nameof(nowUtc));
        if (RevokedAtUtc.HasValue)
            throw new InvalidOperationException("Refresh token has already been revoked.");
        if (ExpiresAtUtc <= nowUtc)
            throw new InvalidOperationException("Expired refresh token cannot be revoked.");
        if (replacedByTokenHash is { Length: > 128 })
            throw new ArgumentException("Replacement token hash is invalid.", nameof(replacedByTokenHash));

        RevokedAtUtc = nowUtc;
        ReplacedByTokenHash = replacedByTokenHash;
        AddDomainEvent(new RefreshTokenRevokedDomainEvent(Id, UserId, nowUtc));
    }
}
