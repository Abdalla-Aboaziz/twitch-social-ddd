namespace TwitchUniConnect.Social.IdentityAndAccessManagment.Application.Contracts;

public sealed record RegisterRequest(string UserName, string Password, string EmailAddress, string PhoneNumber);

public sealed record LoginRequest(string Identifier, string Password);

public sealed record RefreshRequest(string RefreshToken);

public sealed record UpdateAccountRequest(string UserName, string EmailAddress, string PhoneNumber);

public sealed record ChangePasswordRequest(string CurrentPassword, string NewPassword);

public sealed record AuthResponse(Guid UserId, string UserName, string EmailAddress, string AccessToken, DateTime AccessTokenExpiresAtUtc, string RefreshToken, DateTime RefreshTokenExpiresAtUtc);

public sealed record AccountResponse(Guid UserId, string UserName, string EmailAddress, string PhoneNumber, DateTime CreatedAtUtc, DateTime UpdatedAtUtc);
