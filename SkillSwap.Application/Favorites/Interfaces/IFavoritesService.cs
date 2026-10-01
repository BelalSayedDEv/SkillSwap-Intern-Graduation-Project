using SkillSwap.Application.Common.Models;
using SkillSwap.Application.Favorites.DTOs;

namespace SkillSwap.Application.Favorites.Interfaces;

public interface IFavoritesService
{
    Task<Result<ToggleFavoriteResult>> ToggleAsync(Guid userId, ToggleFavoriteRequest request, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<FavoriteDto>>> ListMineAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Result> RemoveAsync(Guid userId, Guid id, CancellationToken cancellationToken = default);
}
