using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreLayer.Entities.Pharmacies;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RepositoryLayer.Data.Configurations.Pharmacy
{
    public class PharmacyListingConfig : IEntityTypeConfiguration<PharmacyListing>
    {
        public void Configure(EntityTypeBuilder<PharmacyListing> builder)
        {
            builder.HasOne(pl => pl.Pharmacy)
                    .WithMany()
                    .HasForeignKey(pl => pl.PharmacyId)
                    .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(pl => pl.Species)
                .WithMany(s => s.PharmacyListings)
                .HasForeignKey(pl => pl.SpeciesId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasIndex(pl => pl.PharmacyId);
            builder.HasIndex(pl => pl.IsActive);
            builder.HasIndex(pl => pl.Category);
        }
    }
}
