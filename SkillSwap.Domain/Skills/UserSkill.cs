namespace SkillSwap.Domain.Skills;

using SkillSwap.Domain.Common;
using SkillSwap.Domain.Identity;

public class UserSkill : BaseEntity
{
    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;
    public Guid SkillId { get; set; }
    public Skill Skill { get; set; } = null!;
    public string Type { get; set; } = "Teach"; // Teach or Learn
    public string Level { get; set; } = "Beginner";
}
