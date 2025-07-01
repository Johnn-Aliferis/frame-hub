using FrameHub.Extensions;
using FrameHub.Model.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FrameHub.Configuration;

public class PhotoConfiguration :  IEntityTypeConfiguration<Photo>
{
    public void Configure(EntityTypeBuilder<Photo> builder)
    {
        builder.ToTable("Photo");
        builder.ConfigureBaseEntity();

        builder.Property(p => p.UserId)
            .IsRequired()
            .HasMaxLength(450);
        
        builder.Property(p => p.StorageKey)
            .IsRequired()
            .HasMaxLength(2048);
        
        builder.Property(p => p.Provider)
            .IsRequired()
            .HasMaxLength(50);
        
        builder.Property(p => p.FileName)
            .IsRequired(false)
            .HasMaxLength(255);
        
        builder.Property(p => p.Tags)
            .IsRequired(false)
            .HasMaxLength(500);

        builder.Property(p => p.IsProfilePicture)
            .IsRequired();
        
        builder.HasOne(p => p.User)
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}