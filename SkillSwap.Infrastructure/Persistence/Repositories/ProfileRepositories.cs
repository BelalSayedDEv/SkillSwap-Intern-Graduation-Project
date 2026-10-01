using Microsoft.EntityFrameworkCore;
using SkillSwap.Application.Profile.Interfaces;
using SkillSwap.Domain.Profile;
using SkillSwap.Infrastructure.Persistence;
using SkillSwap.Infrastructure.Persistence.Repositories;

namespace SkillSwap.Infrastructure.Persistence.Repositories;

public class UserProfileRepository : Repository<UserProfile>, IUserProfileRepository
{
    public UserProfileRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<UserProfile?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _set
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);
    }
}

public class AvailabilityRepository : Repository<Availability>, IAvailabilityRepository
{
    public AvailabilityRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Availability>> ListByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _set.AsNoTracking()
            .Where(a => a.UserId == userId)
            .OrderBy(a => a.DayOfWeek).ThenBy(a => a.StartTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<Availability?> GetUserSlotAsync(Guid userId, Guid id, CancellationToken cancellationToken = default)
    {
        return await _set.FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId, cancellationToken);
    }
}
