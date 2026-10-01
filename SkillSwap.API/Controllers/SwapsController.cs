using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillSwap.API.Extensions;
using SkillSwap.Application.Matches.DTOs;
using SkillSwap.Application.Matches.Interfaces;
using SkillSwap.Domain.Matches;

namespace SkillSwap.API.Controllers;

[Route("api/swaps")]
[ApiController]
[Authorize(Policy = "ActiveUserOnly")]
public class SwapsController : BaseController
{
    private readonly ISwapService _service;

    public SwapsController(ISwapService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> Send(SendSwapRequest request, CancellationToken cancellationToken)
    {
        var result = await _service.SendAsync(User.GetUserId(), request, cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("mine")]
    public async Task<IActionResult> ListMine(
        [FromQuery] string box = "all",
        [FromQuery] SwapStatus? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _service.ListMineAsync(User.GetUserId(), box, status, page, pageSize, cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetDetails(Guid id, CancellationToken cancellationToken)
    {
        var result = await _service.GetDetailsAsync(User.GetUserId(), id, cancellationToken);
        return HandleResult(result);
    }

    [HttpPost("{id:guid}/accept")]
    public async Task<IActionResult> Accept(Guid id, CancellationToken cancellationToken)
    {
        var result = await _service.AcceptAsync(User.GetUserId(), id, cancellationToken);
        return HandleResult(result);
    }

    [HttpPost("{id:guid}/reject")]
    public async Task<IActionResult> Reject(Guid id, CancellationToken cancellationToken)
    {
        var result = await _service.RejectAsync(User.GetUserId(), id, cancellationToken);
        return HandleResult(result);
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
    {
        var result = await _service.CancelAsync(User.GetUserId(), id, cancellationToken);
        return HandleResult(result);
    }
}
