using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreLayer.Entities.Animals;
using CoreLayer.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RepositoryLayer.Data.Configurations.Animals
{
    public class AnimalListingConfig : IEntityTypeConfiguration<AnimalListing>
    {
        public void Configure(EntityTypeBuilder<AnimalListing> builder)
        {
            // Convert Status enum to string
            builder.Property(al => al.Status)
                .HasConversion(
                    status => status.ToString(),// to database
                    status => (ListingStatus)Enum.Parse(typeof(ListingStatus), status)//Take the string value from the database ("Active") and convert it back to the corresponding enum value (ListingStatus.Active).
                )
                .HasMaxLength(50);
            // Convert Status enum to string
            builder.Property(al => al.Type)
                .HasConversion(
                    Type => Type.ToString(),// to database
                    Type => (AnimalListingType)Enum.Parse(typeof(AnimalListingType), Type)//Take the string value from the database ("Active") and convert it back to the corresponding enum value (ListingStatus.Active).
                )
                .HasMaxLength(50);

            builder.HasOne(al => al.Animal)
                .WithMany(a => a.Listings)
                .HasForeignKey(al => al.AnimalId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(al => al.Owner)
                .WithMany(u => u.AnimalListings)
                .HasForeignKey(al => al.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(al => al.Status);
            builder.HasIndex(al => al.CreatedAt);
        }
    }
}
