namespace AuthApi.Models;

public record V2AuthenticatedUserResponse(
    Guid Id,
    string Username);

public record V2AuthSessionResponse(
    string AccessToken,
    int ExpiresIn,
    V2AuthenticatedUserResponse User);

public record V2RefreshResponse(
    string AccessToken,
    int ExpiresIn);

public record V2RefreshValidationResponse(
    bool IsValid,
    Guid UserId,
    DateTime ExpiresAt);