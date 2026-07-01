namespace AuthApi.DTOs;

public record LoginRequest(
    string Username,
    string Password);

public record LoginResponse(
    Guid Id,
    string Token,
    string Role,
    string Username
    );

public record ChangePasswordResponse(
    string Message);

public record PasswordResetCodeResponse(
    string ResetCode,
    DateTime ExpiresAt);

public record ResetCodeRequest(
    string Username);

public record ChangePasswordRequest(
    string Username,
    string ResetCode,
    string NewPassword);

public record CreateUserRequest(
    string Username,
    string Password
    );

public record UserResponse(
    Guid Id,
    string Username,
    string Role);