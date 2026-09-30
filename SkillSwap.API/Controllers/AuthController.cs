using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SkillSwap.Application.Identity.DTOs;
using SkillSwap.Application.Identity.Interfaces;

namespace SkillSwap.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : BaseController
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    [EnableRateLimiting("register")]
    public async Task<IActionResult> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.RegisterAsync(request, cancellationToken);
        return HandleResult(result);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("login")]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.LoginAsync(request, cancellationToken);
        return HandleResult(result);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    [EnableRateLimiting("refresh")]
    public async Task<IActionResult> Refresh(RefreshRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.RefreshTokenAsync(request.RefreshToken, cancellationToken);
        return HandleResult(result);
    }

    [HttpPost("logout")]
    [AllowAnonymous]
    [EnableRateLimiting("logout")]
    public async Task<IActionResult> Logout(RefreshRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.LogoutAsync(request.RefreshToken, cancellationToken);
        return HandleResult(result);
    }

    public record RevokeFamilyRequest(string FamilyId);

    [HttpPost("revoke-family")]
    [Authorize(Policy = "ActiveUserOnly")]
    [EnableRateLimiting("revoke-family")]
    public async Task<IActionResult> RevokeFamily(RevokeFamilyRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.RevokeFamilyAsync(request.FamilyId, cancellationToken);
        return HandleResult(result);
    }
}
