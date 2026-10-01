using SkillSwap.Application.Common.Models;
using SkillSwap.Application.Subscriptions.DTOs;

namespace SkillSwap.Application.Subscriptions.Interfaces;

public interface ISubscriptionService
{
    Task<Result<MySubscriptionDto>> GetMineAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Result<MyQuotaDto>> GetMyQuotaAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Result<MySubscriptionDto>> UpgradeAsync(Guid userId, UpgradeRequest request, CancellationToken cancellationToken = default);
    Task<Result> ConsumeSessionAsync(Guid userId, CancellationToken cancellationToken = default);
}
