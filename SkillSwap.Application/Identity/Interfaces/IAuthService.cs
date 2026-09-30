using SkillSwap.Application.Common.Models;
using SkillSwap.Application.Identity.DTOs;

namespace SkillSwap.Application.Identity.Interfaces;

public interface IAuthService
{
    Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<Result<AuthResponse>> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);

    Task<Result> LogoutAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task<Result> RevokeFamilyAsync(string familyId, CancellationToken cancellationToken = default);
    Task<int> PurgeExpiredTokensAsync(CancellationToken cancellationToken = default);
}
