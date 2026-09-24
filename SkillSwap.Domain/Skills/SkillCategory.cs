namespace SkillSwap.Domain.Skills;

using SkillSwap.Domain.Common;

public class SkillCategory : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? IconUrl { get; set; }
    public ICollection<Skill> Skills { get; set; } = new List<Skill>();
}
