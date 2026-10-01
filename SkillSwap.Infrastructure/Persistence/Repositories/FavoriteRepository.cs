using Microsoft.EntityFrameworkCore;
using SkillSwap.Application.Favorites.Interfaces;
using SkillSwap.Domain.Favorites;
using SkillSwap.Infrastructure.Persistence;
using SkillSwap.Infrastructure.Persistence.Repositories;

namespace SkillSwap.Infrastructure.Persistence.Repositories;

public class FavoriteRepository : Repository<FavoriteUser>, IFavoriteRepository
{
    public FavoriteRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<FavoriteUser?> FindActiveAsync(Guid userId, Guid favoritedId, CancellationToken cancellationToken = default)
    {
        return await _set.FirstOrDefaultAsync(f => f.UserId == userId && f.FavoritedId == favoritedId, cancellationToken);
    }

    public async Task<IReadOnlyList<FavoriteUser>> ListMineAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _set.AsNoTracking()
            .Include(f => f.Favorited).ThenInclude(u => u.Profile)
            .Where(f => f.UserId == userId)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<FavoriteUser?> GetMineAsync(Guid userId, Guid id, CancellationToken cancellationToken = default)
    {
        return await _set
            .Include(f => f.Favorited).ThenInclude(u => u.Profile)
            .FirstOrDefaultAsync(f => f.Id == id && f.UserId == userId, cancellationToken);
    }
}
