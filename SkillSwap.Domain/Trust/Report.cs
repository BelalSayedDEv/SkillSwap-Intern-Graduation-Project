namespace SkillSwap.Domain.Trust;

using SkillSwap.Domain.Common;
using SkillSwap.Domain.Identity;

public class Report : BaseEntity
{
    public Guid ReporterId { get; set; }
    public ApplicationUser Reporter { get; set; } = null!;
    public Guid ReportedUserId { get; set; }
    public ApplicationUser ReportedUser { get; set; } = null!;
    public string Reason { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ReportStatus Status { get; set; } = ReportStatus.Pending;
}
