using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillSwap.Application.Skills.Interfaces;

namespace SkillSwap.API.Controllers;

[Route("api/catalog")]
[ApiController]
[AllowAnonymous]
public class SkillCatalogController : BaseController
{
    private readonly ISkillCatalogService _catalog;

    public SkillCatalogController(ISkillCatalogService catalog)
    {
        _catalog = catalog;
    }

    [HttpGet("categories")]
    public async Task<IActionResult> ListCategories(CancellationToken cancellationToken)
    {
        var result = await _catalog.ListCategoriesAsync(cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("skills")]
    public async Task<IActionResult> ListSkills(
        [FromQuery] int? categoryId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _catalog.ListApprovedSkillsAsync(categoryId, page, pageSize, cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("skills/search")]
    public async Task<IActionResult> SearchSkills(
        [FromQuery] string query = "",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _catalog.SearchSkillsAsync(query, page, pageSize, cancellationToken);
        return HandleResult(result);
    }
}
