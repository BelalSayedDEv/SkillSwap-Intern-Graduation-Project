using SkillSwap.Application.Favorites.Interfaces;
using SkillSwap.Application.Identity.Interfaces;
using SkillSwap.Application.Matches.Interfaces;
using SkillSwap.Application.Profile.Interfaces;
using SkillSwap.Application.Skills.Interfaces;
using SkillSwap.Application.Subscriptions.Interfaces;
using SkillSwap.Application.Trust.Interfaces;

namespace SkillSwap.Application.Common.Interfaces;

public interface IUnitOfWork : IDisposable
{
    ISkillCategoryRepository SkillCategories { get; }
    ISkillRepository Skills { get; }
    IUserSkillRepository UserSkills { get; }
    IUserProfileRepository Profiles { get; }
    IAvailabilityRepository Availabilities { get; }
    IFavoriteRepository Favorites { get; }
    IUserRepository Users { get; }
    ISwapRepository Swaps { get; }
    IBlockRepository Blocks { get; }
    IReportRepository Reports { get; }
    ISubscriptionRepository Subscriptions { get; }
    IQuotaRepository Quotas { get; }

    Task<int> CompleteAsync(CancellationToken cancellationToken = default);
}
