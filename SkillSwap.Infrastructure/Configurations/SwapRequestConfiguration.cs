namespace SkillSwap.Infrastructure.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SkillSwap.Domain.Matches;

public class SwapRequestConfiguration : IEntityTypeConfiguration<SwapRequest>
{
    public void Configure(EntityTypeBuilder<SwapRequest> builder)
    {
        builder.ToTable("SwapRequests");

        builder.Property(s => s.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(s => s.Message)
            .HasMaxLength(1000);

        builder.HasOne(s => s.Sender)
            .WithMany(u => u.SwapRequestsSent)
            .HasForeignKey(s => s.SenderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Receiver)
            .WithMany()
            .HasForeignKey(s => s.ReceiverId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.SkillOffered)
            .WithMany()
            .HasForeignKey(s => s.SkillOfferedId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.SkillWanted)
            .WithMany()
            .HasForeignKey(s => s.SkillWantedId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(s => new { s.SenderId, s.Status });
        builder.HasIndex(s => new { s.ReceiverId, s.Status });

        builder.HasQueryFilter(s => !s.IsDeleted);
    }
}
