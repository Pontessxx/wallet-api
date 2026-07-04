namespace AuthApi.DTOs;

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