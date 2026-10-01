using SkillSwap.Application.Common.Models;
using SkillSwap.Application.Matches.DTOs;
using SkillSwap.Domain.Matches;

namespace SkillSwap.Application.Matches.Interfaces;

public interface ISwapService
{
    Task<Result<SwapDto>> SendAsync(Guid senderId, SendSwapRequest request, CancellationToken cancellationToken = default);
    Task<Result<SwapDto>> GetDetailsAsync(Guid userId, Guid id, CancellationToken cancellationToken = default);
    Task<Result<PagedResult<SwapDto>>> ListMineAsync(Guid userId, string box, SwapStatus? status, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<Result<SwapDto>> AcceptAsync(Guid userId, Guid id, CancellationToken cancellationToken = default);
    Task<Result<SwapDto>> RejectAsync(Guid userId, Guid id, CancellationToken cancellationToken = default);
    Task<Result> CancelAsync(Guid userId, Guid id, CancellationToken cancellationToken = default);
}
