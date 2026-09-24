namespace SkillSwap.Domain.Subscriptions;

using SkillSwap.Domain.Common;

public class SessionQuota : BaseEntity
{
    public Guid SubscriptionId { get; set; }
    public Subscription Subscription { get; set; } = null!;
    public DateTime CycleStartDate { get; set; }
    public DateTime CycleEndDate { get; set; }
    public int IncludedSessions { get; set; }
    public int AddOnSessions { get; set; } = 0;
    public int UsedSessions { get; set; } = 0;
}
