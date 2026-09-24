namespace SkillSwap.Infrastructure.Persistence;

using System.Reflection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SkillSwap.Domain.Favorites;
using SkillSwap.Domain.Identity;
using SkillSwap.Domain.Matches;
using SkillSwap.Domain.Profile;
using SkillSwap.Domain.Skills;
using SkillSwap.Domain.Subscriptions;
using SkillSwap.Domain.Trust;

public class AppDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // NOTE: Users DbSet is inherited from IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>.

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<Availability> Availabilities => Set<Availability>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<SessionQuota> SessionQuotas => Set<SessionQuota>();
    public DbSet<SkillCategory> SkillCategories => Set<SkillCategory>();
    public DbSet<Skill> Skills => Set<Skill>();
    public DbSet<UserSkill> UserSkills => Set<UserSkill>();
    public DbSet<Report> Reports => Set<Report>();
    public DbSet<Block> Blocks => Set<Block>();
    public DbSet<FavoriteUser> FavoriteUsers => Set<FavoriteUser>();
    public DbSet<SwapRequest> SwapRequests => Set<SwapRequest>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
