namespace SkillSwap.Domain.Skills;

using SkillSwap.Domain.Common;

public class Skill : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public SkillCategory Category { get; set; } = null!;
    public bool IsApproved { get; set; } = false;
}
