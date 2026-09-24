namespace SkillSwap.Infrastructure.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SkillSwap.Domain.Subscriptions;

public class SessionQuotaConfiguration : IEntityTypeConfiguration<SessionQuota>
{
    public void Configure(EntityTypeBuilder<SessionQuota> builder)
    {
        builder.ToTable("SessionQuotas");

        builder.HasOne(q => q.Subscription)
            .WithMany(s => s.Quotas)
            .HasForeignKey(q => q.SubscriptionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(q => !q.IsDeleted);
    }
}
