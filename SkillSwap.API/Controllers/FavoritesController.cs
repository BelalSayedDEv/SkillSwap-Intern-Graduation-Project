using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillSwap.API.Extensions;
using SkillSwap.Application.Favorites.DTOs;
using SkillSwap.Application.Favorites.Interfaces;

namespace SkillSwap.API.Controllers;

[Route("api/me/favorites")]
[ApiController]
[Authorize(Policy = "ActiveUserOnly")]
public class FavoritesController : BaseController
{
    private readonly IFavoritesService _service;

    public FavoritesController(IFavoritesService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> ListMine(CancellationToken cancellationToken)
    {
        var result = await _service.ListMineAsync(User.GetUserId(), cancellationToken);
        return HandleResult(result);
    }

    [HttpPost("toggle")]
    public async Task<IActionResult> Toggle(ToggleFavoriteRequest request, CancellationToken cancellationToken)
    {
        var result = await _service.ToggleAsync(User.GetUserId(), request, cancellationToken);
        return HandleResult(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Remove(Guid id, CancellationToken cancellationToken)
    {
        var result = await _service.RemoveAsync(User.GetUserId(), id, cancellationToken);
        return HandleResult(result);
    }
}
