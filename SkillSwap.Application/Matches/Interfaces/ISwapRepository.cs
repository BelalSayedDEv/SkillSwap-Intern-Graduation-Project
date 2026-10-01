using SkillSwap.Application.Common.Interfaces;
using SkillSwap.Application.Common.Models;
using SkillSwap.Domain.Matches;

namespace SkillSwap.Application.Matches.Interfaces;

public interface ISwapRepository : IRepository<SwapRequest>
{
    Task<PagedResult<SwapRequest>> ListMineAsync(Guid userId, string box, SwapStatus? status, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<SwapRequest?> GetMineAsync(Guid userId, Guid id, CancellationToken cancellationToken = default);
}
