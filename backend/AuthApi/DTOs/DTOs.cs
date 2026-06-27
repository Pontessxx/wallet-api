namespace AuthApi.DTOs;

public record LoginRequest(
    string Username,
    string Password);

public record LoginResponse(
    string Token,
    string Role,
    string Username
    );