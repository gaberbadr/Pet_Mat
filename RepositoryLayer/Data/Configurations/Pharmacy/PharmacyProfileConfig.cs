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
    public class PharmacyProfileConfig : IEntityTypeConfiguration<PharmacyProfile>
    {
        public void Configure(EntityTypeBuilder<PharmacyProfile> builder)
        {
            builder.HasIndex(pp => pp.UserId).IsUnique();
            builder.HasIndex(pp => pp.IsActive);
            builder.HasIndex(pp => new { pp.Latitude, pp.Longitude });

            builder.HasMany(pp => pp.Ratings)
                  .WithOne()
                  .HasForeignKey(pr => pr.PharmacyId)
                  .HasPrincipalKey(pp => pp.UserId);  // Link PharmacyId (FK) in Ratings to UserId (Alternate Key) in PharmacyProfile
        }
    }
}
