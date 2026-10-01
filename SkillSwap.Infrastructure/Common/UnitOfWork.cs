using SkillSwap.Application.Common.Interfaces;
using SkillSwap.Application.Favorites.Interfaces;
using SkillSwap.Application.Identity.Interfaces;
using SkillSwap.Application.Matches.Interfaces;
using SkillSwap.Application.Profile.Interfaces;
using SkillSwap.Application.Skills.Interfaces;
using SkillSwap.Application.Subscriptions.Interfaces;
using SkillSwap.Application.Trust.Interfaces;
using SkillSwap.Infrastructure.Persistence;

namespace SkillSwap.Infrastructure.Common;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public UnitOfWork(
        AppDbContext context,
        ISkillCategoryRepository skillCategories,
        ISkillRepository skills,
        IUserSkillRepository userSkills,
        IUserProfileRepository profiles,
        IAvailabilityRepository availabilities,
        IFavoriteRepository favorites,
        IUserRepository users,
        ISwapRepository swaps,
        IBlockRepository blocks,
        IReportRepository reports,
        ISubscriptionRepository subscriptions,
        IQuotaRepository quotas)
    {
        _context = context;
        SkillCategories = skillCategories;
        Skills = skills;
        UserSkills = userSkills;
        Profiles = profiles;
        Availabilities = availabilities;
        Favorites = favorites;
        Users = users;
        Swaps = swaps;
        Blocks = blocks;
        Reports = reports;
        Subscriptions = subscriptions;
        Quotas = quotas;
    }

    public ISkillCategoryRepository SkillCategories { get; }
    public ISkillRepository Skills { get; }
    public IUserSkillRepository UserSkills { get; }
    public IUserProfileRepository Profiles { get; }
    public IAvailabilityRepository Availabilities { get; }
    public IFavoriteRepository Favorites { get; }
    public IUserRepository Users { get; }
    public ISwapRepository Swaps { get; }
    public IBlockRepository Blocks { get; }
    public IReportRepository Reports { get; }
    public ISubscriptionRepository Subscriptions { get; }
    public IQuotaRepository Quotas { get; }

    public async Task<int> CompleteAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
