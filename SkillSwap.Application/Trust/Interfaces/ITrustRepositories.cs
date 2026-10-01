using SkillSwap.Application.Common.Interfaces;
using SkillSwap.Application.Common.Models;
using SkillSwap.Domain.Trust;

namespace SkillSwap.Application.Trust.Interfaces;

public interface IBlockRepository
{
    Task<bool> IsBlockedAsync(Guid userA, Guid userB, CancellationToken cancellationToken = default);
    Task<Block?> FindMineAsync(Guid blockerId, Guid blockedId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Block>> ListMineAsync(Guid blockerId, CancellationToken cancellationToken = default);
    Task AddAsync(Block block, CancellationToken cancellationToken = default);
}

public interface IReportRepository : IRepository<Report>
{
    Task<PagedResult<Report>> GetPagedAsync(ReportStatus? status, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<Report?> GetPendingByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsPendingAsync(Guid reporterId, Guid reportedUserId, CancellationToken cancellationToken = default);
}
