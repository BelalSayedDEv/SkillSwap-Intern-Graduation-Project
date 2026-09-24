namespace SkillSwap.Domain.Favorites;

using SkillSwap.Domain.Common;
using SkillSwap.Domain.Identity;

public class FavoriteUser : BaseEntity
{
    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;
    public Guid FavoritedId { get; set; }
    public ApplicationUser Favorited { get; set; } = null!;
}
