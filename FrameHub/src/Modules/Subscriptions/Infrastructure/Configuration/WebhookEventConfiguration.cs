using FrameHub.Modules.Subscriptions.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FrameHub.Modules.Subscriptions.Infrastructure.Configuration;

public class WebhookEventConfiguration : IEntityTypeConfiguration<WebhookEvent>
{
    public void Configure(EntityTypeBuilder<WebhookEvent> builder)
    {
        builder.ToTable("WebhookEvent");
        
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.EventId)
            .HasMaxLength(100)
            .IsRequired();
        
        builder.Property(e => e.EventType)
            .HasMaxLength(100)
            .IsRequired();
        
        builder.Property(e => e.CustomerEmail)
            .HasMaxLength(254)
            .IsRequired(false);

        builder.Property(e => e.ReceivedAt)
            .HasDefaultValueSql("GETUTCDATE()")
            .IsRequired();
        
        builder.Property(e => e.RawPayload)
            .HasColumnType("nvarchar(max)")
            .IsRequired();
    }
}