namespace SkillSwap.Domain.Trust;

using SkillSwap.Domain.Common;
using SkillSwap.Domain.Identity;

public class Block : BaseEntity
{
    public Guid BlockerId { get; set; }
    public ApplicationUser Blocker { get; set; } = null!;
    public Guid BlockedId { get; set; }
    public ApplicationUser BlockedUser { get; set; } = null!;
}
