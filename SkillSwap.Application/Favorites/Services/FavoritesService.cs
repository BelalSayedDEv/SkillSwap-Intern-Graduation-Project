using SkillSwap.Application.Common.Interfaces;
using SkillSwap.Application.Common.Models;
using SkillSwap.Application.Favorites.DTOs;
using SkillSwap.Application.Favorites.Interfaces;
using SkillSwap.Application.Identity.Interfaces;
using SkillSwap.Domain.Favorites;

namespace SkillSwap.Application.Favorites.Services;

public class FavoritesService : IFavoritesService
{
    private readonly IUnitOfWork _uow;
    private readonly IUserRepository _users;

    public FavoritesService(IUnitOfWork uow, IUserRepository users)
    {
        _uow = uow;
        _users = users;
    }

    public async Task<Result<ToggleFavoriteResult>> ToggleAsync(Guid userId, ToggleFavoriteRequest request, CancellationToken cancellationToken = default)
    {
        if (request.FavoritedId == userId)
            return Result<ToggleFavoriteResult>.Failure(ErrorType.Validation, "You cannot favorite yourself.");

        if (!await _users.ExistsActiveAsync(request.FavoritedId, cancellationToken))
            return Result<ToggleFavoriteResult>.Failure(ErrorType.NotFound, "User not found.");

        var existing = await _uow.Favorites.FindActiveAsync(userId, request.FavoritedId, cancellationToken);
        if (existing is not null)
        {
            existing.IsDeleted = true;
            existing.DeletedAt = DateTime.UtcNow;
            await _uow.CompleteAsync(cancellationToken);
            return Result<ToggleFavoriteResult>.Success(new ToggleFavoriteResult(false, null));
        }

        var entity = new FavoriteUser
        {
            UserId = userId,
            FavoritedId = request.FavoritedId
        };

        await _uow.Favorites.AddAsync(entity, cancellationToken);
        await _uow.CompleteAsync(cancellationToken);

        var profile = await _uow.Profiles.GetByUserIdAsync(request.FavoritedId, cancellationToken);
        var name = profile?.FullName ?? string.Empty;
        return Result<ToggleFavoriteResult>.Success(new ToggleFavoriteResult(true, new FavoriteDto(entity.Id, entity.FavoritedId, name)));
    }

    public async Task<Result<IReadOnlyList<FavoriteDto>>> ListMineAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var items = await _uow.Favorites.ListMineAsync(userId, cancellationToken);
        return Result<IReadOnlyList<FavoriteDto>>.Success(
            items.Select(f => new FavoriteDto(f.Id, f.FavoritedId, f.Favorited?.Profile?.FullName ?? string.Empty)).ToList());
    }

    public async Task<Result> RemoveAsync(Guid userId, Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _uow.Favorites.GetMineAsync(userId, id, cancellationToken);
        if (entity is null)
            return Result.Failure(ErrorType.NotFound, "Favorite not found.");

        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;

        await _uow.CompleteAsync(cancellationToken);
        return Result.Success();
    }
}
