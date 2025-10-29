using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreLayer.Entities.Accessories;
using CoreLayer.Entities.Foods;
using CoreLayer.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RepositoryLayer.Data.Configurations.Accessories
{
    public class AccessoryListingConfig : IEntityTypeConfiguration<AccessoryListing>
    {
        public void Configure(EntityTypeBuilder<AccessoryListing> builder)
        {
            // Convert Status enum to string
            builder.Property(al => al.Status)
                .HasConversion(
                    status => status.ToString(),// to database
                    status => (ListingStatus)Enum.Parse(typeof(ListingStatus), status)//Take the string value from the database ("Active") and convert it back to the corresponding enum value (ListingStatus.Active).
                )
                .HasMaxLength(50);

            // Convert Condition enum to string
            builder.Property(al => al.Condition)
                .HasConversion(
                    condition => condition.ToString(),
                    condition => (AccessoryCondition)Enum.Parse(typeof(AccessoryCondition), condition)
                )
                .HasMaxLength(50);

            // Convert Category enum to string
            builder.Property(al => al.Category)
                .HasConversion(
                    category => category.ToString(),
                    category => (AccessoryCategory)Enum.Parse(typeof(AccessoryCategory), category)
                )
                .HasMaxLength(100);

            // Configure relationships
            builder.HasOne(al => al.Owner)
                .WithMany(u => u.AccessoryListings)
                .HasForeignKey(al => al.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(al => al.Species)
                .WithMany(s => s.AccessoryListings)
                .HasForeignKey(al => al.SpeciesId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configure indexes
            builder.HasIndex(al => al.OwnerId);
            builder.HasIndex(al => al.Status);
            builder.HasIndex(al => al.Category);
            builder.HasIndex(al => al.IsActive);
            builder.HasIndex(al => al.CreatedAt); 
        }
    }
}
