using FrameHub.Extensions;
using FrameHub.Model.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FrameHub.Configuration;

public class UserSubscriptionConfiguration : IEntityTypeConfiguration<UserSubscription>
{
    public void Configure(EntityTypeBuilder<UserSubscription> builder)
    {
        builder.ToTable("UserSubscription");
        builder.ConfigureBaseEntity();
        
        
        builder.Property(us => us.UserId)
            .IsRequired();

        builder.Property(us => us.SubscriptionPlanId)
            .IsRequired();

        builder.Property(us => us.AssignedAt)
            .IsRequired()
            .HasColumnType("datetime2");
        
        builder.Property(us => us.ExpiresAt)
            .IsRequired()
            .HasColumnType("datetime2");
        
        // Enforce uniqueness
        builder.HasIndex(us => us.UserId)
            .IsUnique();
        
        
        builder.HasOne(us => us.User)
            .WithOne(u => u.Subscription)
            .HasForeignKey<UserSubscription>(us => us.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(us => us.SubscriptionPlan)
            .WithOne(p => p.UserSubscription)
            .HasForeignKey<UserSubscription>(us => us.SubscriptionPlanId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}