namespace SkillSwap.Application.Identity.DTOs;

public record AuthResponse(
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiresAt
    );

public record LoginRequest(
    string Email,
    string Password
    );

public record RegisterRequest(
    string FullName,
    string Email,
    string Password
    );

public record RefreshRequest(string RefreshToken);
