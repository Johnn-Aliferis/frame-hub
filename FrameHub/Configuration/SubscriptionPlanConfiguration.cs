using FrameHub.Extensions;
using FrameHub.Model.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FrameHub.Configuration;

public class SubscriptionPlanConfiguration : IEntityTypeConfiguration<SubscriptionPlan>
{
    public void Configure(EntityTypeBuilder<SubscriptionPlan> builder)
    {
        builder.ToTable("SubscriptionPlan");
        builder.ConfigureBaseEntity();
        
        builder.Property(sp => sp.Code)
            .IsRequired()
            .HasMaxLength(20);
        
        // Enforce uniqueness
        builder.HasIndex(sp => sp.Code)
            .IsUnique();
        
        builder.Property(sp => sp.Name)
            .IsRequired()
            .HasMaxLength(50);
        
        builder.Property(sp => sp.ProductId)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(sp => sp.PriceId)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(sp => sp.Description)
            .IsRequired(false)
            .HasMaxLength(200);

        builder.Property(sp => sp.MaxUploads)
            .IsRequired();
        
        builder.Property(sp => sp.PlanOrder)
            .IsRequired();

        builder.Property(sp => sp.MonthlyPrice)
            .IsRequired()
            .HasColumnType("decimal(10,2)");
    }
}