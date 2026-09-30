namespace SkillSwap.Domain.Skills;

using SkillSwap.Domain.Common;
using SkillSwap.Domain.Identity;

public class UserSkill : BaseEntity
{
    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;
    public int SkillId { get; set; }
    public Skill Skill { get; set; } = null!;
    public SkillType Type { get; set; } = SkillType.Teach;
    public SkillLevel Level { get; set; } = SkillLevel.Beginner;
}
