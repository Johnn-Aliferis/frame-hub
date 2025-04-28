using FrameHub.Extensions;
using FrameHub.Model.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FrameHub.Configuration;

public class UserInfoConfiguration : IEntityTypeConfiguration<UserInfo>
{
    public void Configure(EntityTypeBuilder<UserInfo> builder)
    {
        builder.ToTable("UserInfo");
        builder.ConfigureBaseEntity();
        
        builder.Property(ui => ui.DisplayName)
            .IsRequired()
            .HasMaxLength(50);
        
        builder.Property(ui => ui.PhoneNumber)
            .IsRequired(false)
            .HasMaxLength(20);
        
        builder.Property(ui => ui.Bio)
            .IsRequired(false)
            .HasMaxLength(500);
        
        builder.Property(ui => ui.UserId)
            .IsRequired();
        
        builder.Property(ui => ui.ProfilePictureId)
            .IsRequired(false);
        
        // Enforce uniqueness
        builder.HasIndex(ui => ui.UserId)
            .IsUnique();
        
        
        builder.HasOne(ui => ui.User)
            .WithOne(u => u.Info)
            .HasForeignKey<UserInfo>(ui => ui.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(ui => ui.ProfilePicture)
            .WithOne()
            .HasForeignKey<UserInfo>(ui => ui.ProfilePictureId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}