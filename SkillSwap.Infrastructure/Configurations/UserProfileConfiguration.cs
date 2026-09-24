namespace SkillSwap.Infrastructure.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SkillSwap.Domain.Profile;

public class UserProfileConfiguration : IEntityTypeConfiguration<UserProfile>
{
    public void Configure(EntityTypeBuilder<UserProfile> builder)
    {
        builder.ToTable("UserProfiles");

        builder.Property(p => p.FullName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.Bio)
            .HasMaxLength(1000);

        builder.Property(p => p.ProfilePictureUrl)
            .HasMaxLength(500);

        builder.Property(p => p.Location)
            .HasMaxLength(200);

        builder.Property(p => p.Timezone)
            .HasMaxLength(100);

        builder.Property(p => p.Latitude)
            .HasPrecision(9, 6);

        builder.Property(p => p.Longitude)
            .HasPrecision(9, 6);

        builder.HasOne(p => p.User)
            .WithOne(u => u.Profile)
            .HasForeignKey<UserProfile>(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(p => !p.IsDeleted);
    }
}
