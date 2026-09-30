using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillSwap.Application.Skills.DTOs;
using SkillSwap.Application.Skills.Interfaces;

namespace SkillSwap.API.Controllers;

[Route("api/moderation/skills")]
[ApiController]
public class SkillModerationController : BaseController
{
    private readonly ISkillModerationService _moderation;

    public SkillModerationController(ISkillModerationService moderation)
    {
        _moderation = moderation;
    }

    [HttpPost("requests")]
    [Authorize(Policy = "ActiveUserOnly")]
    public async Task<IActionResult> RequestSkill(RequestSkillRequest request, CancellationToken cancellationToken)
    {
        var result = await _moderation.RequestSkillAsync(request, cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("pending")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> ListPending(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _moderation.ListPendingAsync(page, pageSize, cancellationToken);
        return HandleResult(result);
    }

    [HttpPost("{id:int}/approve")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Approve(int id, CancellationToken cancellationToken)
    {
        var result = await _moderation.ApproveAsync(id, cancellationToken);
        return HandleResult(result);
    }

    [HttpPost("{id:int}/reject")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Reject(int id, CancellationToken cancellationToken)
    {
        var result = await _moderation.RejectAsync(id, cancellationToken);
        return HandleResult(result);
    }
}
