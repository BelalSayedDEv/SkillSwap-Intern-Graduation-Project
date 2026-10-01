using SkillSwap.Application.Common.Interfaces;
using SkillSwap.Application.Common.Models;
using SkillSwap.Application.Subscriptions.DTOs;
using SkillSwap.Application.Subscriptions.Interfaces;
using SkillSwap.Domain.Subscriptions;

namespace SkillSwap.Application.Subscriptions.Services;

public class SubscriptionService : ISubscriptionService
{
    private readonly IUnitOfWork _uow;

    public SubscriptionService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Result<MySubscriptionDto>> GetMineAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var (sub, _) = await EnsureActiveAsync(userId, cancellationToken);
        return Result<MySubscriptionDto>.Success(Map(sub));
    }

    public async Task<Result<MyQuotaDto>> GetMyQuotaAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var (sub, quota) = await EnsureActiveAsync(userId, cancellationToken);
        return Result<MyQuotaDto>.Success(MapQuota(sub.Id, quota));
    }

    public async Task<Result<MySubscriptionDto>> UpgradeAsync(Guid userId, UpgradeRequest request, CancellationToken cancellationToken = default)
    {
        if (!Enum.IsDefined(request.PlanType))
            return Result<MySubscriptionDto>.Failure(ErrorType.Validation, "Invalid plan.");

        var now = DateTime.UtcNow;
        var current = await _uow.Subscriptions.GetActiveByUserAsync(userId, cancellationToken);
        if (current is not null)
        {
            current.IsActive = false;
            current.EndDate = now;
            current.UpdatedAt = now;
        }

        var sub = new Subscription
        {
            UserId = userId,
            PlanType = request.PlanType,
            StartDate = now,
            EndDate = null,
            IsActive = true
        };
        await _uow.Subscriptions.AddAsync(sub, cancellationToken);

        var (start, end) = MonthCycle(now);
        await _uow.Quotas.AddAsync(new SessionQuota
        {
            SubscriptionId = sub.Id,
            CycleStartDate = start,
            CycleEndDate = end,
            IncludedSessions = SessionsFor(request.PlanType),
            AddOnSessions = 0,
            UsedSessions = 0
        }, cancellationToken);

        await _uow.CompleteAsync(cancellationToken);
        return Result<MySubscriptionDto>.Success(Map(sub));
    }

    public async Task<Result> ConsumeSessionAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var (_, quota) = await EnsureActiveAsync(userId, cancellationToken);
        var remaining = quota.IncludedSessions + quota.AddOnSessions - quota.UsedSessions;
        if (remaining <= 0)
            return Result.Failure(ErrorType.LogicError, "No sessions left in the current cycle.");

        quota.UsedSessions++;
        await _uow.CompleteAsync(cancellationToken);
        return Result.Success();
    }

    private async Task<(Subscription Sub, SessionQuota Quota)> EnsureActiveAsync(Guid userId, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var sub = await _uow.Subscriptions.GetActiveByUserAsync(userId, cancellationToken);
        if (sub is null)
        {
            sub = new Subscription
            {
                UserId = userId,
                PlanType = SubscriptionPlan.Free,
                StartDate = now,
                IsActive = true
            };
            await _uow.Subscriptions.AddAsync(sub, cancellationToken);
        }

        var quota = await _uow.Quotas.GetCurrentAsync(sub.Id, now, cancellationToken);
        if (quota is null)
        {
            var (start, end) = MonthCycle(now);
            quota = new SessionQuota
            {
                SubscriptionId = sub.Id,
                CycleStartDate = start,
                CycleEndDate = end,
                IncludedSessions = SessionsFor(sub.PlanType),
                AddOnSessions = 0,
                UsedSessions = 0
            };
            await _uow.Quotas.AddAsync(quota, cancellationToken);
        }

        await _uow.CompleteAsync(cancellationToken);
        return (sub, quota);
    }

    private static (DateTime Start, DateTime End) MonthCycle(DateTime now)
    {
        var start = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var end = start.AddMonths(1).AddTicks(-1);
        return (start, end);
    }

    private static int SessionsFor(SubscriptionPlan plan)
    {
        return plan == SubscriptionPlan.Premium ? 30 : 5;
    }

    private static MySubscriptionDto Map(Subscription s)
    {
        return new MySubscriptionDto(s.Id, s.PlanType, s.PlanType.ToString(), s.StartDate, s.EndDate, s.IsActive);
    }

    private static MyQuotaDto MapQuota(Guid subId, SessionQuota q)
    {
        return new MyQuotaDto(subId, q.IncludedSessions, q.AddOnSessions, q.UsedSessions,
            q.IncludedSessions + q.AddOnSessions - q.UsedSessions, q.CycleStartDate, q.CycleEndDate);
    }
}
