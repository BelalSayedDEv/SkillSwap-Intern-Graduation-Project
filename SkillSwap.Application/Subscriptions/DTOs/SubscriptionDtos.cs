using SkillSwap.Domain.Subscriptions;

namespace SkillSwap.Application.Subscriptions.DTOs;

public record MySubscriptionDto(
    Guid Id,
    SubscriptionPlan PlanType,
    string PlanName,
    DateTime? StartDate,
    DateTime? EndDate,
    bool IsActive
    );

public record MyQuotaDto(
    Guid SubscriptionId,
    int IncludedSessions,
    int AddOnSessions,
    int UsedSessions,
    int RemainingSessions,
    DateTime CycleStartDate,
    DateTime CycleEndDate
    );

public record UpgradeRequest(
    SubscriptionPlan PlanType
    );
