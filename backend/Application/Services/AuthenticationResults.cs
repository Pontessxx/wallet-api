namespace Application.Services;

public record AuthenticatedUserResult(
    Guid Id,
    string Username);

public record AuthSessionResult(
    string AccessToken,
    int ExpiresIn,
    AuthenticatedUserResult User,
    string RefreshToken,
    DateTime RefreshTokenExpiresAt);