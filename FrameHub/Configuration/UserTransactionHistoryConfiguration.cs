using FrameHub.Extensions;
using FrameHub.Model.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FrameHub.Configuration;

public class UserTransactionHistoryConfiguration  : IEntityTypeConfiguration<UserTransactionHistory>
{
    public void Configure(EntityTypeBuilder<UserTransactionHistory> builder)
    {
        builder.ToTable("UserTransactionHistory");
        builder.ConfigureBaseEntity();
        
        builder.Property(uth => uth.UserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(uth => uth.Amount)
            .HasColumnType("decimal(10,2)")
            .IsRequired();
        
        builder.Property(uth => uth.Currency)
            .HasMaxLength(10)
            .IsRequired(false);
        
        builder.Property(uth => uth.InvoiceId)
            .HasMaxLength(255)
            .IsRequired(false);

        builder.Property(uth => uth.Description)
            .HasMaxLength(255)
            .IsRequired(true);

        builder.Property(uth => uth.ReceiptUrl)
            .HasMaxLength(2048)
            .IsRequired(false);
        
        builder.Property(uth => uth.PlanPriceId)
            .HasMaxLength(50)
            .IsRequired(false);
        
        builder.HasOne(uth => uth.User)
            .WithMany()
            .HasForeignKey(uth => uth.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}