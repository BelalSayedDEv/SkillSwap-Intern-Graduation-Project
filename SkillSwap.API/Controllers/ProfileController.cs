using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillSwap.API.Extensions;
using SkillSwap.Application.Profile.DTOs;
using SkillSwap.Application.Profile.Interfaces;

namespace SkillSwap.API.Controllers;

[Route("api/me/profile")]
[ApiController]
[Authorize(Policy = "ActiveUserOnly")]
public class ProfileController : BaseController
{
    private readonly IProfileService _service;

    public ProfileController(IProfileService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetMine(CancellationToken cancellationToken)
    {
        var result = await _service.GetMyProfileAsync(User.GetUserId(), cancellationToken);
        return HandleResult(result);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateMine(UpdateMyProfileRequest request, CancellationToken cancellationToken)
    {
        var result = await _service.UpdateMyProfileAsync(User.GetUserId(), request, cancellationToken);
        return HandleResult(result);
    }

    [HttpPost("complete-onboarding")]
    public async Task<IActionResult> CompleteOnboarding(CancellationToken cancellationToken)
    {
        var result = await _service.CompleteOnboardingAsync(User.GetUserId(), cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("availabilities")]
    public async Task<IActionResult> ListMyAvailabilities(CancellationToken cancellationToken)
    {
        var result = await _service.ListMyAvailabilitiesAsync(User.GetUserId(), cancellationToken);
        return HandleResult(result);
    }

    [HttpPost("availabilities")]
    public async Task<IActionResult> SetAvailability(SetAvailabilityRequest request, CancellationToken cancellationToken)
    {
        var result = await _service.SetAvailabilityAsync(User.GetUserId(), request, cancellationToken);
        return HandleResult(result);
    }

    [HttpDelete("availabilities/{id:guid}")]
    public async Task<IActionResult> RemoveAvailability(Guid id, CancellationToken cancellationToken)
    {
        var result = await _service.RemoveAvailabilityAsync(User.GetUserId(), id, cancellationToken);
        return HandleResult(result);
    }
}
