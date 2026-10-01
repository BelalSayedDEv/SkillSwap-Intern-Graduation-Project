using SkillSwap.Application.Common.Interfaces;
using SkillSwap.Domain.Favorites;

namespace SkillSwap.Application.Favorites.Interfaces;

public interface IFavoriteRepository : IRepository<FavoriteUser>
{
    Task<FavoriteUser?> FindActiveAsync(Guid userId, Guid favoritedId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<FavoriteUser>> ListMineAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<FavoriteUser?> GetMineAsync(Guid userId, Guid id, CancellationToken cancellationToken = default);
}
