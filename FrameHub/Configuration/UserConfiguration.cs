using FrameHub.Extensions;
using FrameHub.Model.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FrameHub.Configuration;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.ConfigureBaseEntity();

        builder.Property(u => u.LastLogin)
            .IsRequired(false)
            .HasColumnType("datetime2");
        
        // Navigation relationships
        builder.HasOne(u => u.Credential)
            .WithOne(c => c.User)
            .HasForeignKey<UserCredential>(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(u => u.Info)
            .WithOne(i => i.User)
            .HasForeignKey<UserInfo>(i => i.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(u => u.Subscription)
            .WithOne(s => s.User)
            .HasForeignKey<UserSubscription>(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.Photos)
            .WithOne(p => p.User)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}