using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SkillSwap.Application.Common.Models;
using SkillSwap.Application.Identity.DTOs;
using SkillSwap.Application.Identity.Interfaces;
using SkillSwap.Domain.Identity;
using SkillSwap.Domain.Profile;
using SkillSwap.Infrastructure.Authentication;
using SkillSwap.Infrastructure.Persistence;
using System.Security.Cryptography;
using System.Text;

namespace SkillSwap.Infrastructure.Services.Auth;
public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly AppDbContext _dbContext;
    private readonly IOptionsMonitor<JwtSettings> _jwtOptions;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        ITokenService tokenService,
        AppDbContext dbContext,
        IOptionsMonitor<JwtSettings> jwtOptions)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _dbContext = dbContext;
        _jwtOptions = jwtOptions;
    }

    public async Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);

        if (existingUser != null)
        {
            return Result<AuthResponse>.Failure(ErrorType.Conflict, "Email is already registered.");
        }

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            Status = UserStatus.Active,
            Role = UserRole.User
        };

        await using var tx = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var createResult = await _userManager.CreateAsync(user, request.Password);

            if (!createResult.Succeeded)
            {
                var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                return Result<AuthResponse>.Failure(ErrorType.Validation, errors);
            }

            var profile = new UserProfile
            {
                UserId = user.Id,
                FullName = request.FullName
            };

            _dbContext.UserProfiles.Add(profile);

            var now = DateTime.UtcNow;
            var accessToken = _tokenService.GenerateAccessToken(user.Id, user.Email!, user.Role.ToString());
            var refreshToken = _tokenService.GenerateRefreshToken();
            var hashedRefreshToken = HashToken(refreshToken);

            var tokenEntity = new RefreshToken
            {
                UserId = user.Id,
                TokenHash = hashedRefreshToken,
                ExpiresAt = now.AddDays(_jwtOptions.CurrentValue.RefreshTokenDays),
                FamilyId = Guid.NewGuid().ToString()
            };

            _dbContext.RefreshTokens.Add(tokenEntity);
            await _dbContext.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);

            return Result<AuthResponse>.Success(new AuthResponse(
                accessToken,
                refreshToken,
                now.AddMinutes(_jwtOptions.CurrentValue.AccessTokenMinutes)
            ));
        }
        catch
        {
            await tx.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
        {
            return Result<AuthResponse>.Failure(ErrorType.Unauthorized, "Invalid email or password.");
        }

        if (user.Status != UserStatus.Active)
        {
            return Result<AuthResponse>.Failure(ErrorType.LogicError, "User account is suspended or inactive.");
        }

        var now = DateTime.UtcNow;
        var accessToken = _tokenService.GenerateAccessToken(user.Id, user.Email!, user.Role.ToString());
        var refreshToken = _tokenService.GenerateRefreshToken();
        var hashedRefreshToken = HashToken(refreshToken);

        var tokenEntity = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = hashedRefreshToken,
            ExpiresAt = now.AddDays(_jwtOptions.CurrentValue.RefreshTokenDays),
            FamilyId = Guid.NewGuid().ToString()
        };

        _dbContext.RefreshTokens.Add(tokenEntity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result<AuthResponse>.Success(new AuthResponse(
            accessToken,
            refreshToken,
            now.AddMinutes(_jwtOptions.CurrentValue.AccessTokenMinutes)
        ));
    }

    public async Task<Result<AuthResponse>> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var hashedInput = HashToken(refreshToken);

        var existingToken = await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(t => t.TokenHash == hashedInput, cancellationToken);

        if (existingToken == null || existingToken.ExpiresAt < DateTime.UtcNow)
        {
            return Result<AuthResponse>.Failure(ErrorType.Unauthorized, "Invalid or expired refresh token.");
        }

        if (existingToken.RevokedAt != null)
        {
            await RevokeFamilyInternalAsync(existingToken.FamilyId, cancellationToken);
            return Result<AuthResponse>.Failure(ErrorType.Unauthorized, "Refresh token reuse detected. All sessions revoked.");
        }

        var user = await _userManager.FindByIdAsync(existingToken.UserId.ToString());

        if (user == null || user.Status != UserStatus.Active)
        {
            return Result<AuthResponse>.Failure(ErrorType.LogicError, "User account is suspended or inactive.");
        }

        var now = DateTime.UtcNow;
        var newAccessToken = _tokenService.GenerateAccessToken(user.Id, user.Email!, user.Role.ToString());
        var newRefreshToken = _tokenService.GenerateRefreshToken();
        var hashedNewRefreshToken = HashToken(newRefreshToken);

        existingToken.RevokedAt = now;
        existingToken.ReplacedByTokenHash = hashedNewRefreshToken;

        var newTokenEntity = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = hashedNewRefreshToken,
            ExpiresAt = now.AddDays(_jwtOptions.CurrentValue.RefreshTokenDays),
            FamilyId = existingToken.FamilyId
        };

        _dbContext.RefreshTokens.Add(newTokenEntity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result<AuthResponse>.Success(new AuthResponse(
            newAccessToken,
            newRefreshToken,
            now.AddMinutes(_jwtOptions.CurrentValue.AccessTokenMinutes)
        ));
    }

    public async Task<Result> LogoutAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            return Result.Success();

        var hashedInput = HashToken(refreshToken);
        var token = await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(t => t.TokenHash == hashedInput, cancellationToken);

        if (token is null || token.RevokedAt != null)
            return Result.Success();

        token.RevokedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> RevokeFamilyAsync(string familyId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(familyId))
            return Result.Failure(ErrorType.Validation, "FamilyId is required.");

        await RevokeFamilyInternalAsync(familyId, cancellationToken);
        return Result.Success();
    }




    public async Task<int> PurgeExpiredTokensAsync(CancellationToken cancellationToken = default)
    {
        var cutoff = DateTime.UtcNow.AddDays(-30);
        return await _dbContext.RefreshTokens
            .Where(t => t.ExpiresAt < cutoff)
            .ExecuteDeleteAsync(cancellationToken);
    }

    private async Task RevokeFamilyInternalAsync(string familyId, CancellationToken cancellationToken)
    {
        await _dbContext.RefreshTokens
            .Where(t => t.FamilyId == familyId && t.RevokedAt == null)
            .ExecuteUpdateAsync(
                s => s.SetProperty(t => t.RevokedAt, DateTime.UtcNow),
                cancellationToken);
    }

    private static string HashToken(string token)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(token);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
}
