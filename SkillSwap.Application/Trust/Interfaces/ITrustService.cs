using SkillSwap.Application.Common.Models;
using SkillSwap.Application.Trust.DTOs;

namespace SkillSwap.Application.Trust.Interfaces;

public interface ITrustService
{
    Task<Result<BlockedUserDto>> BlockAsync(Guid blockerId, BlockUserRequest request, CancellationToken cancellationToken = default);
    Task<Result> UnblockAsync(Guid blockerId, Guid blockedId, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<BlockedUserDto>>> ListMyBlocksAsync(Guid blockerId, CancellationToken cancellationToken = default);
    Task<Result<ReportDto>> ReportAsync(Guid reporterId, ReportUserRequest request, CancellationToken cancellationToken = default);
    Task<Result<PagedResult<ReportDto>>> ListReportsAsync(Domain.Trust.ReportStatus? status, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<Result<ReportDto>> ResolveAsync(Guid id, ResolveReportRequest request, CancellationToken cancellationToken = default);
}
