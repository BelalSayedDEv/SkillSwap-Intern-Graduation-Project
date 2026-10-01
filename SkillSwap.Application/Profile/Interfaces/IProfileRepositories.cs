using SkillSwap.Application.Common.Interfaces;
using SkillSwap.Domain.Profile;

namespace SkillSwap.Application.Profile.Interfaces;

public interface IUserProfileRepository : IRepository<UserProfile>
{
    Task<UserProfile?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}

public interface IAvailabilityRepository : IRepository<Availability>
{
    Task<IReadOnlyList<Availability>> ListByUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Availability?> GetUserSlotAsync(Guid userId, Guid id, CancellationToken cancellationToken = default);
}
