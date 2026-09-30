using SkillSwap.Application.Common.Models;
using SkillSwap.Application.Skills.DTOs;

namespace SkillSwap.Application.Skills.Interfaces;

public interface ISkillModerationService
{
    Task<Result<RequestedSkillDto>> RequestSkillAsync(RequestSkillRequest request, CancellationToken cancellationToken = default);
    Task<Result<PagedResult<PendingSkillDto>>> ListPendingAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<Result<SkillDto>> ApproveAsync(int id, CancellationToken cancellationToken = default);
    Task<Result> RejectAsync(int id, CancellationToken cancellationToken = default);
}
