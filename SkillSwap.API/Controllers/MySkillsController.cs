using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillSwap.API.Extensions;
using SkillSwap.Application.Skills.DTOs;
using SkillSwap.Application.Skills.Interfaces;

namespace SkillSwap.API.Controllers;

[Route("api/me/skills")]
[ApiController]
[Authorize(Policy = "ActiveUserOnly")]
public class MySkillsController : BaseController
{
    private readonly IMySkillsService _service;

    public MySkillsController(IMySkillsService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> ListMine(CancellationToken cancellationToken)
    {
        var result = await _service.ListMineAsync(User.GetUserId(), cancellationToken);
        return HandleResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> Add(AddMySkillRequest request, CancellationToken cancellationToken)
    {
        var result = await _service.AddAsync(User.GetUserId(), request, cancellationToken);
        return HandleResult(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Remove(Guid id, CancellationToken cancellationToken)
    {
        var result = await _service.RemoveAsync(User.GetUserId(), id, cancellationToken);
        return HandleResult(result);
    }
}
