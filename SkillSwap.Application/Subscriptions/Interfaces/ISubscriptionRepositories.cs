using SkillSwap.Application.Common.Interfaces;
using SkillSwap.Domain.Subscriptions;

namespace SkillSwap.Application.Subscriptions.Interfaces;

public interface ISubscriptionRepository : IRepository<Subscription>
{
    Task<Subscription?> GetActiveByUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Subscription>> ListByUserAsync(Guid userId, CancellationToken cancellationToken = default);
}

public interface IQuotaRepository : IRepository<SessionQuota>
{
    Task<SessionQuota?> GetCurrentAsync(Guid subscriptionId, DateTime now, CancellationToken cancellationToken = default);
}
