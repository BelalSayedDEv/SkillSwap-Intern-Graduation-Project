using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillSwap.API.Extensions;
using SkillSwap.Application.Subscriptions.DTOs;
using SkillSwap.Application.Subscriptions.Interfaces;

namespace SkillSwap.API.Controllers;

[Route("api/me/subscription")]
[ApiController]
[Authorize(Policy = "ActiveUserOnly")]
public class SubscriptionController : BaseController
{
    private readonly ISubscriptionService _service;

    public SubscriptionController(ISubscriptionService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetMine(CancellationToken cancellationToken)
    {
        var result = await _service.GetMineAsync(User.GetUserId(), cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("quota")]
    public async Task<IActionResult> GetQuota(CancellationToken cancellationToken)
    {
        var result = await _service.GetMyQuotaAsync(User.GetUserId(), cancellationToken);
        return HandleResult(result);
    }

    [HttpPost("upgrade")]
    public async Task<IActionResult> Upgrade(UpgradeRequest request, CancellationToken cancellationToken)
    {
        var result = await _service.UpgradeAsync(User.GetUserId(), request, cancellationToken);
        return HandleResult(result);
    }
}
