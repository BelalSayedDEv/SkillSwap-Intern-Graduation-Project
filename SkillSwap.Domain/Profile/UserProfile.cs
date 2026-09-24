namespace SkillSwap.Domain.Profile;

using SkillSwap.Domain.Common;
using SkillSwap.Domain.Identity;

public class UserProfile : BaseEntity
{
    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;
    public string FullName { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public string? Location { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public string? Timezone { get; set; }
    public bool IsOnboardingCompleted { get; set; } = false;
}
