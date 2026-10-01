using Microsoft.EntityFrameworkCore;
using SkillSwap.Application.Subscriptions.Interfaces;
using SkillSwap.Domain.Subscriptions;
using SkillSwap.Infrastructure.Persistence;
using SkillSwap.Infrastructure.Persistence.Repositories;

namespace SkillSwap.Infrastructure.Persistence.Repositories;

public class SubscriptionRepository : Repository<Subscription>, ISubscriptionRepository
{
    public SubscriptionRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Subscription?> GetActiveByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _set
            .Include(s => s.Quotas)
            .Where(s => s.UserId == userId && s.IsActive)
            .OrderByDescending(s => s.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Subscription>> ListByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _set.AsNoTracking()
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}

public class QuotaRepository : Repository<SessionQuota>, IQuotaRepository
{
    public QuotaRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<SessionQuota?> GetCurrentAsync(Guid subscriptionId, DateTime now, CancellationToken cancellationToken = default)
    {
        return await _set.FirstOrDefaultAsync(
            q => q.SubscriptionId == subscriptionId && q.CycleStartDate <= now && now <= q.CycleEndDate,
            cancellationToken);
    }
}
