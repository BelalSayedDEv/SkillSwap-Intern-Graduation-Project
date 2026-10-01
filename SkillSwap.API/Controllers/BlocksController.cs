using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillSwap.API.Extensions;
using SkillSwap.Application.Trust.DTOs;
using SkillSwap.Application.Trust.Interfaces;

namespace SkillSwap.API.Controllers;

[Route("api/me/blocks")]
[ApiController]
[Authorize(Policy = "ActiveUserOnly")]
public class BlocksController : BaseController
{
    private readonly ITrustService _service;

    public BlocksController(ITrustService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> ListMine(CancellationToken cancellationToken)
    {
        var result = await _service.ListMyBlocksAsync(User.GetUserId(), cancellationToken);
        return HandleResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> Block(BlockUserRequest request, CancellationToken cancellationToken)
    {
        var result = await _service.BlockAsync(User.GetUserId(), request, cancellationToken);
        return HandleResult(result);
    }

    [HttpDelete("{blockedId:guid}")]
    public async Task<IActionResult> Unblock(Guid blockedId, CancellationToken cancellationToken)
    {
        var result = await _service.UnblockAsync(User.GetUserId(), blockedId, cancellationToken);
        return HandleResult(result);
    }
}
