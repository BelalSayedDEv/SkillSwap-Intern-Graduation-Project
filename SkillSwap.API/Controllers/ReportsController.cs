using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillSwap.API.Extensions;
using SkillSwap.Application.Trust.DTOs;
using SkillSwap.Application.Trust.Interfaces;
using SkillSwap.Domain.Trust;

namespace SkillSwap.API.Controllers;

[ApiController]
public class ReportsController : BaseController
{
    private readonly ITrustService _service;

    public ReportsController(ITrustService service)
    {
        _service = service;
    }

    [HttpPost("api/reports")]
    [Authorize(Policy = "ActiveUserOnly")]
    public async Task<IActionResult> Report(ReportUserRequest request, CancellationToken cancellationToken)
    {
        var result = await _service.ReportAsync(User.GetUserId(), request, cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("api/admin/reports")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> ListReports(
        [FromQuery] ReportStatus? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _service.ListReportsAsync(status, page, pageSize, cancellationToken);
        return HandleResult(result);
    }

    [HttpPost("api/admin/reports/{id:guid}/resolve")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Resolve(Guid id, ResolveReportRequest request, CancellationToken cancellationToken)
    {
        var result = await _service.ResolveAsync(id, request, cancellationToken);
        return HandleResult(result);
    }
}
