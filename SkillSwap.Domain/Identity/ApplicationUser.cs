namespace SkillSwap.Domain.Identity;

using Microsoft.AspNetCore.Identity;
using SkillSwap.Domain.Favorites;
using SkillSwap.Domain.Matches;
using SkillSwap.Domain.Profile;
using SkillSwap.Domain.Skills;
using SkillSwap.Domain.Subscriptions;
using SkillSwap.Domain.Trust;

public class ApplicationUser : IdentityUser<Guid>
{
    public UserRole Role { get; set; } = UserRole.User;
    public UserStatus Status { get; set; } = UserStatus.Active;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; } = false;

    // Navigations
    public UserProfile? Profile { get; set; }
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public ICollection<Availability> Availabilities { get; set; } = new List<Availability>();
    public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    public ICollection<UserSkill> Skills { get; set; } = new List<UserSkill>();
    public ICollection<Report> ReportsMade { get; set; } = new List<Report>();
    public ICollection<Block> BlocksInitiated { get; set; } = new List<Block>();
    public ICollection<FavoriteUser> Favorites { get; set; } = new List<FavoriteUser>();
    public ICollection<SwapRequest> SwapRequestsSent { get; set; } = new List<SwapRequest>();
}
