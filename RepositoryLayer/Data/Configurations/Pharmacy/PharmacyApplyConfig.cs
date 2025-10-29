using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreLayer.Entities.Pharmacies;
using CoreLayer.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RepositoryLayer.Data.Configurations.Pharmacy
{
    public class PharmacyApplyConfig : IEntityTypeConfiguration<PharmacyApply>
    {
        public void Configure(EntityTypeBuilder<PharmacyApply> builder)
        {

            // Convert Status enum to string
            builder.Property(pa => pa.Status)
                .HasConversion(
                    status => status.ToString(),// to database
                    status => (ApplicationStatus)Enum.Parse(typeof(ApplicationStatus), status)//Take the string value from the database ("Active") and convert it back to the corresponding enum value (ListingStatus.Active).
                )
                .HasMaxLength(50);

            builder.HasIndex(pa => pa.UserId).IsUnique();
            builder.HasIndex(pa => pa.Status);
        }
    }
}
