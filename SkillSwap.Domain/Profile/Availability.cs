namespace SkillSwap.Domain.Profile;

using SkillSwap.Domain.Common;
using SkillSwap.Domain.Identity;

public class Availability : BaseEntity
{
    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;
    public int DayOfWeek { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public bool IsActive { get; set; } = true;
}
