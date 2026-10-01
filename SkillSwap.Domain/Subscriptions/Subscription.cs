namespace SkillSwap.Domain.Subscriptions;

using SkillSwap.Domain.Common;
using SkillSwap.Domain.Identity;

public class Subscription : BaseEntity
{
    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;
    public SubscriptionPlan PlanType { get; set; } = SubscriptionPlan.Free;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<SessionQuota> Quotas { get; set; } = new List<SessionQuota>();
}
