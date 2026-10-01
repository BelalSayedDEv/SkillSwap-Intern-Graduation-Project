namespace SkillSwap.Application.Favorites.DTOs;

public record ToggleFavoriteRequest(
    Guid FavoritedId
    );

public record FavoriteDto(
    Guid Id,
    Guid FavoritedId,
    string FavoritedName
    );

public record ToggleFavoriteResult(
    bool Added,
    FavoriteDto? Favorite
    );
