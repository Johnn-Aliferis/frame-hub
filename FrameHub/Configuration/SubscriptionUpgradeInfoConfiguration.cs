using FrameHub.Extensions;
using FrameHub.Model.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FrameHub.Configuration;

public class SubscriptionUpgradeInfoConfiguration  : IEntityTypeConfiguration<SubscriptionUpgradeInfo>
{
    public void Configure(EntityTypeBuilder<SubscriptionUpgradeInfo> builder)
    {
        builder.ToTable("SubscriptionUpgradeInfo");
        builder.ConfigureBaseEntity();
        
        
        builder.Property(us => us.UserId)
            .IsRequired()
            .HasMaxLength(450);
        
        builder.Property(us => us.SubscriptionId)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(us => us.PreviousPriceId)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(us => us.PreviousPeriodEnd)
            .IsRequired()
            .HasColumnType("datetime2");
        
        builder.Property(us => us.NewPriceId)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(us => us.UpgradeStatus)
            .IsRequired()
            .HasMaxLength(50);
        
        builder.HasOne(backup => backup.User)
            .WithMany() 
            .HasForeignKey(backup => backup.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}