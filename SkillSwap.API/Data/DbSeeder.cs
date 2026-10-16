using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SkillSwap.Domain.Identity;
using SkillSwap.Domain.Matches;
using SkillSwap.Domain.Profile;
using SkillSwap.Domain.Skills;
using SkillSwap.Infrastructure.Persistence;

namespace SkillSwap.API.Data;

// Development-only seed data for frontend testing (Swagger + React demo).
// Idempotent: safe to run on every startup, inserts only what is missing.
public static class DbSeeder
{
    public const string AdminEmail = "admin@skillswap.local";
    public const string MemberEmail = "member@skillswap.local";
    public const string AdminPassword = "Admin123!";
    public const string MemberPassword = "Member123!";

    public static async Task SeedAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        var context = services.GetRequiredService<AppDbContext>();
        var users = services.GetRequiredService<UserManager<ApplicationUser>>();
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("DbSeeder");

        await SeedCatalogAsync(context, cancellationToken);
        var admin = await EnsureUserAsync(users, context, AdminEmail, AdminPassword, "Site Admin",
            UserRole.Admin, "Cairo, Egypt", cancellationToken);
        var member = await EnsureUserAsync(users, context, MemberEmail, MemberPassword, "Karim Hassan",
            UserRole.User, "Giza, Egypt", cancellationToken);

        await SeedUserContentAsync(context, admin, member, cancellationToken);
        logger.LogInformation("Seed data ready (admin + member + catalog)");
    }

    private static async Task SeedCatalogAsync(AppDbContext context, CancellationToken ct)
    {
        if (await context.SkillCategories.AnyAsync(ct))
            return;

        var catalog = new Dictionary<string, (string[] Approved, string[] Pending)>
        {
            ["Design"] = (new[] { "Photoshop", "Figma", "Illustrator", "UI Design", "Logo Design" }, new[] { "Motion Graphics" }),
            ["Programming"] = (new[] { "C#", "Python", "JavaScript", "SQL", "ASP.NET Core" }, new[] { "Rust" }),
            ["Languages"] = (new[] { "English", "Arabic", "German", "French" }, new[] { "Japanese" }),
            ["Music"] = (new[] { "Guitar", "Piano", "Singing" }, Array.Empty<string>()),
            ["Cooking"] = (new[] { "Baking", "Italian Cuisine", "Grilling" }, Array.Empty<string>()),
            ["Photography"] = (new[] { "Portrait", "Lightroom", "Street Photography" }, Array.Empty<string>()),
            ["Marketing"] = (new[] { "SEO", "Copywriting", "Social Media Ads" }, Array.Empty<string>()),
            ["Sports"] = (new[] { "Swimming", "Yoga", "Football Coaching" }, Array.Empty<string>()),
        };

        foreach (var (categoryName, skills) in catalog)
        {
            var category = new SkillCategory { Name = categoryName };
            context.SkillCategories.Add(category);
            await context.SaveChangesAsync(ct);

            foreach (var name in skills.Approved)
                context.Skills.Add(new Skill { Name = name, CategoryId = category.Id, IsApproved = true });
            foreach (var name in skills.Pending)
                context.Skills.Add(new Skill { Name = name, CategoryId = category.Id, IsApproved = false });
        }

        await context.SaveChangesAsync(ct);
    }

    private static async Task<ApplicationUser> EnsureUserAsync(
        UserManager<ApplicationUser> users, AppDbContext context,
        string email, string password, string fullName, UserRole role, string location,
        CancellationToken ct)
    {
        var user = await users.FindByEmailAsync(email);
        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                Role = role,
                Status = UserStatus.Active
            };
            var result = await users.CreateAsync(user, password);
            result.ThrowIfFailed();

            context.UserProfiles.Add(new UserProfile
            {
                UserId = user.Id,
                FullName = fullName,
                Bio = role == UserRole.Admin ? "Platform administrator" : "Learning and teaching on SkillSwap",
                Location = location,
                Timezone = "Africa/Cairo",
                IsOnboardingCompleted = true
            });
            await context.SaveChangesAsync(ct);
        }

        return user;
    }

    private static async Task SeedUserContentAsync(
        AppDbContext context, ApplicationUser admin, ApplicationUser member, CancellationToken ct)
    {
        var photoshop = await context.Skills.FirstOrDefaultAsync(s => s.Name == "Photoshop", ct);
        var figma = await context.Skills.FirstOrDefaultAsync(s => s.Name == "Figma", ct);
        var csharp = await context.Skills.FirstOrDefaultAsync(s => s.Name == "C#", ct);
        var guitar = await context.Skills.FirstOrDefaultAsync(s => s.Name == "Guitar", ct);
        if (photoshop is null || figma is null || csharp is null || guitar is null)
            return;

        if (!await context.UserSkills.AnyAsync(ct))
        {
            context.UserSkills.AddRange(
                new UserSkill { UserId = member.Id, SkillId = photoshop.Id, Type = SkillType.Teach, Level = SkillLevel.Advanced },
                new UserSkill { UserId = member.Id, SkillId = csharp.Id, Type = SkillType.Learn, Level = SkillLevel.Beginner },
                new UserSkill { UserId = admin.Id, SkillId = figma.Id, Type = SkillType.Teach, Level = SkillLevel.Expert },
                new UserSkill { UserId = admin.Id, SkillId = guitar.Id, Type = SkillType.Teach, Level = SkillLevel.Intermediate });
        }

        if (!await context.Availabilities.AnyAsync(ct))
        {
            context.Availabilities.AddRange(
                new Availability { UserId = member.Id, DayOfWeek = 6, StartTime = new TimeOnly(18, 0), EndTime = new TimeOnly(21, 0), IsActive = true },
                new Availability { UserId = member.Id, DayOfWeek = 3, StartTime = new TimeOnly(18, 0), EndTime = new TimeOnly(20, 0), IsActive = true });
        }

        if (!await context.FavoriteUsers.AnyAsync(ct))
        {
            context.FavoriteUsers.Add(new Domain.Favorites.FavoriteUser { UserId = member.Id, FavoritedId = admin.Id });
        }

        if (!await context.SwapRequests.AnyAsync(ct))
        {
            context.SwapRequests.Add(new SwapRequest
            {
                SenderId = member.Id,
                ReceiverId = admin.Id,
                SkillOfferedId = photoshop.Id,
                SkillWantedId = figma.Id,
                Message = "Hi! I can teach Photoshop, would love to learn Figma.",
                Status = SwapStatus.Pending
            });
        }

        await context.SaveChangesAsync(ct);
    }

    private static void ThrowIfFailed(this IdentityResult result)
    {
        if (!result.Succeeded)
            throw new InvalidOperationException("Seed user creation failed: " +
                string.Join(", ", result.Errors.Select(e => e.Description)));
    }
}
