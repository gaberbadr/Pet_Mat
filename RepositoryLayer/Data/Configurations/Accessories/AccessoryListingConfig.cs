using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreLayer.Entities.Accessories;
using CoreLayer.Entities.Foods;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RepositoryLayer.Data.Configurations.Accessories
{
    public class AccessoryListingConfig : IEntityTypeConfiguration<AccessoryListing>
    {
        public void Configure(EntityTypeBuilder<AccessoryListing> builder)
        {
            builder.HasOne(al => al.Owner)
                    .WithMany(u => u.AccessoryListings)
                    .HasForeignKey(al => al.OwnerId)
                    .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(al => al.Species)
                .WithMany(s => s.AccessoryListings)
                .HasForeignKey(al => al.SpeciesId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasIndex(al => al.OwnerId);
            builder.HasIndex(al => al.Status);
            builder.HasIndex(al => al.Category);
            builder.HasIndex(al => new { al.Latitude, al.Longitude });
        }
    }
}
