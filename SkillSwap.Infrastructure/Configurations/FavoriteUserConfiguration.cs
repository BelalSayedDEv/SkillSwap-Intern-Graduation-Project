namespace SkillSwap.Infrastructure.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SkillSwap.Domain.Favorites;

public class FavoriteUserConfiguration : IEntityTypeConfiguration<FavoriteUser>
{
    public void Configure(EntityTypeBuilder<FavoriteUser> builder)
    {
        builder.ToTable("FavoriteUsers");

        builder.HasOne(f => f.User)
            .WithMany(u => u.Favorites)
            .HasForeignKey(f => f.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(f => f.Favorited)
            .WithMany()
            .HasForeignKey(f => f.FavoritedId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(f => new { f.UserId, f.FavoritedId })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

        builder.HasQueryFilter(f => !f.IsDeleted);
    }
}
