using FrameHub.Model.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FrameHub.Configuration;

public class WebhookEventConfiguration : IEntityTypeConfiguration<WebhookEvent>
{
    public void Configure(EntityTypeBuilder<WebhookEvent> builder)
    {
        builder.ToTable("StripeWebhookEvent");
        
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.EventId)
            .HasMaxLength(100)
            .IsRequired();
        
        builder.Property(e => e.EventType)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.ReceivedAt)
            .HasDefaultValueSql("GETUTCDATE()")
            .IsRequired();
        
        builder.Property(e => e.RawPayload)
            .HasColumnType("nvarchar(max)")
            .IsRequired();

        builder.Property(e => e.Processed)
            .HasDefaultValue(true)
            .IsRequired();
    }
}