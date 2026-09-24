namespace SkillSwap.Domain.Matches;

using SkillSwap.Domain.Common;
using SkillSwap.Domain.Identity;
using SkillSwap.Domain.Skills;

public class SwapRequest : BaseEntity
{
    public Guid SenderId { get; set; }
    public ApplicationUser Sender { get; set; } = null!;
    public Guid ReceiverId { get; set; }
    public ApplicationUser Receiver { get; set; } = null!;
    public Guid SkillOfferedId { get; set; }
    public Skill SkillOffered { get; set; } = null!;
    public Guid SkillWantedId { get; set; }
    public Skill SkillWanted { get; set; } = null!;
    public Guid? ScheduledSessionId { get; set; }
    public string? Message { get; set; }
    public string Status { get; set; } = "Pending";
}
