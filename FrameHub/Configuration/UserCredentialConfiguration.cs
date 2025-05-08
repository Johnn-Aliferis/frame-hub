using FrameHub.Extensions;
using FrameHub.Model.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FrameHub.Configuration;

public class UserCredentialConfiguration : IEntityTypeConfiguration<UserCredential>
{
    public void Configure(EntityTypeBuilder<UserCredential> builder)
    {
        builder.ToTable("UserCredential");
        
        builder.ConfigureBaseEntity();
        
        builder.Property(uc => uc.Email)
            .IsRequired()
            .HasMaxLength(100);

        // Enforce uniqueness
        builder.HasIndex(uc => uc.Email)
            .IsUnique();
        
        builder.HasIndex(uc => uc.UserId)
            .IsUnique();

        builder.Property(uc => uc.PasswordHash)
            .IsRequired()
            .HasMaxLength(256);
        
        builder.Property(uc => uc.Provider)
            .IsRequired()
            .HasMaxLength(50);
        
        builder.Property(uc => uc.ExternalId)
            .IsRequired(false)
            .HasMaxLength(100);

        builder.Property(uc => uc.UserId)
            .IsRequired();

        builder.HasOne(uc => uc.User)
            .WithOne(u => u.Credential)
            .HasForeignKey<UserCredential>(uc => uc.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}